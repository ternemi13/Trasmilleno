using System.Collections;
using UnityEngine;
namespace Transmilleno
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : Actor
    {
        public enum MoveState { Grounded, Airborne, Vaulting, Fence, Boarded, Dead }
        public MoveState State {get; private set;} public Stamina Stamina {get;} = new Stamina();
        public GameInput Input {get; private set;} public Transform View, Hand; public PoseDriver Pose;
        public bool HasKnife;public GameObject KnifeVisual; public string Prompt {get; private set;} public float Speed {get; private set;}
        CharacterController motor; Vector3 velocity; float vertical, groundedAt, jumpAt = -100, attackAt, lastGroundY; int combo; Pickable held; Climbable fence; float fenceTop; Vector3 fenceNormal; readonly Collider[] hits = new Collider[24]; readonly Actor[] victims = new Actor[24];
        void Awake() { motor = GetComponent<CharacterController>(); Input = new GameInput(); }
        void Start() { Session.Player = this; Health = 100; HasKnife = Session.Level && Session.Level.knife; lastGroundY = transform.position.y; }
        void OnDestroy() { Input?.Dispose(); if(Session.Player == this) Session.Player = null; }
        void Update()
        {
            if(KnifeVisual)KnifeVisual.SetActive(HasKnife&&!held);if (!Session.Running || IsDead || !View) return;
            Stamina.Tick(Time.deltaTime, Session.Difficulty?Session.Difficulty.stamina:1);
            if (State == MoveState.Boarded || State == MoveState.Vaulting) return;
            if (State == MoveState.Fence) { UpdateFence(); return; }
            Vector2 move = Input.Move.ReadValue<Vector2>(); Vector3 forward = Vector3.ProjectOnPlane(View.forward, Vector3.up).normalized; Vector3 right = Vector3.Cross(Vector3.up, forward); Vector3 direction = Vector3.ClampMagnitude(forward * move.y + right * move.x, 1);
            bool grounded = motor.isGrounded || Physics.SphereCast(transform.position + Vector3.up * .32f, .22f, Vector3.down, out _, .19f, 1 << 0, QueryTriggerInteraction.Ignore);
            if (grounded) { groundedAt = Time.time; if (vertical < 0) { float fall = lastGroundY - transform.position.y; if (fall > 4) TakeDamage(new DamageInfo((fall - 4) * 18, DamageType.Fall, gameObject)); vertical = -2; lastGroundY = transform.position.y; } State = MoveState.Grounded; } else { State = MoveState.Airborne; lastGroundY = Mathf.Max(lastGroundY, transform.position.y); }
            if (Input.Jump.WasPressedThisFrame()) { jumpAt = Time.time; if (TryClimb()) return; }
            if (Time.time - jumpAt < .15f && Time.time - groundedAt < .12f && Stamina.Spend(5)) { vertical = Mathf.Sqrt(2 * 24 * 1.2f); jumpAt = -100; groundedAt = -100; }
            bool sprint = direction.sqrMagnitude > .05f && Input.Sprint.IsPressed() && Stamina.Spend(17 * Time.deltaTime);
            float targetSpeed = Input.Crouch.IsPressed() ? 1.3f : sprint ? 5.5f : 2.2f;
            velocity = Vector3.MoveTowards(velocity, direction * targetSpeed, Time.deltaTime * 18); vertical -= 24 * Time.deltaTime;
            motor.Move((velocity + Vector3.up * vertical) * Time.deltaTime); Speed = velocity.magnitude;
            if (direction.sqrMagnitude > .01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 14);
            if(Pose) { Pose.Speed = Speed; Pose.Climb = 0; Pose.Holding = held; }
            if (Input.Punch.WasPressedThisFrame()) { if (held) { held.Throw((View.forward + Vector3.up * .19f).normalized * 16, gameObject); held = null; Stamina.Spend(8); } else Attack(false); }
            if (Input.Knife.WasPressedThisFrame() && HasKnife) Attack(true);
            if (Input.Interact.WasPressedThisFrame()) PickUp();
            Prompt = "";
            if (Physics.Raycast(transform.position + Vector3.up * .65f, transform.forward, out var hit, 1.25f, 1 << 0) && hit.collider.GetComponentInParent<Climbable>()) Prompt = "ESPACIO   Trepar / saltar";
            int n = Physics.OverlapSphereNonAlloc(transform.position, 1.7f, hits); for(int i=0;i<n;i++) if(hits[i].GetComponent<Pickable>()) { Prompt = "E   Recoger objeto"; break; }
            if (transform.position.y < -12) TakeDamage(new DamageInfo(999, DamageType.Fall, gameObject));
            if (transform.position.x > -3.2f && transform.position.x < 3.2f && Mathf.Abs(transform.position.z) < 20 && !Session.Trespassing) { Session.Trespassing = true; if(Session.Route == "Sin registrar") Session.Route = "Cruce de vía"; GameEvents.Notify("Ya estás dentro. Busca las puertas del próximo bus."); }
        }
        public bool TryClimb()
        {
            if (!Physics.SphereCast(transform.position + Vector3.up * .7f, .17f, transform.forward, out var h, 1.1f, 1 << 0, QueryTriggerInteraction.Ignore)) return false;
            var c = h.collider.GetComponentInParent<Climbable>(); if (!c || Stamina.Value < 18) return false;
            float top = h.collider.bounds.max.y; float height = top - transform.position.y;
            if (c.fence && height > 1.25f) { fence = c; fenceTop = top; fenceNormal = h.normal; State = MoveState.Fence; velocity = Vector3.zero; vertical = 0; transform.rotation = Quaternion.LookRotation(-h.normal); GameEvents.Notify("WASD   Escalar reja   ·   ESPACIO   Soltarse"); return true; }
            if (height > 2.5f || height < .45f || !c.hasTopClearance) return false;
            Vector3 dest = transform.position + transform.forward * (h.collider.bounds.size.z < 1.5f ? 1.65f : 1.2f); dest.y = top + .05f;
            if (Physics.CheckCapsule(dest + Vector3.up * .4f, dest + Vector3.up * 1.5f, .25f, 1 << 0, QueryTriggerInteraction.Ignore)) return false;
            if(!Stamina.Spend(18)) return false; StartCoroutine(Vault(dest, top, c.route)); return true;
        }
        IEnumerator Vault(Vector3 destination, float top, string route)
        {
            State = MoveState.Vaulting; motor.enabled = false; var start = transform.position; float t = 0; if(Pose)Pose.Climb = 1;
            while(t < 1) { t += Time.deltaTime / .72f; var p = Vector3.Lerp(start, destination, Mathf.SmoothStep(0,1,t)); p.y = Mathf.Lerp(start.y,destination.y,t) + Mathf.Sin(t * Mathf.PI) * .65f; transform.position = p; yield return null; }
            transform.position = destination; motor.enabled = true; vertical = -2; if(Pose)Pose.Climb = 0; State = MoveState.Airborne; Session.Route = route; GameEvents.EmitNoise(transform.position, 6);
        }
        void UpdateFence()
        {
            if(!fence || Input.Jump.WasPressedThisFrame() || !Stamina.Spend(13 * Time.deltaTime)) { State = MoveState.Airborne; if(Pose)Pose.Climb = 0; vertical = 0; return; }
            Vector2 m = Input.Move.ReadValue<Vector2>(); motor.Move((Vector3.up * m.y + transform.right * m.x) * (1.35f * Time.deltaTime)); if(Pose) {Pose.Climb=1;Pose.Speed=2;}
            if(transform.position.y > fenceTop - 1.1f && m.y > 0) { if(fence.hasTopClearance && !Physics.Raycast(transform.position + Vector3.up * 1.7f, Vector3.up, .8f,1<<0)) StartCoroutine(Vault(new Vector3(transform.position.x-fenceNormal.x*1.25f,fenceTop+.05f,transform.position.z-fenceNormal.z*1.25f), fenceTop, "Reja")); else { motor.Move(Vector3.down * .03f); GameEvents.Notify("La cubierta bloquea el paso. Busca otro tramo."); } }
            if(motor.isGrounded && m.y < 0) {State=MoveState.Grounded;if(Pose)Pose.Climb=0;}
        }
        void Attack(bool knife)
        {
            if(Time.time < attackAt || !Stamina.Spend(knife ? 11 : 7))return; combo=(combo+1)%3;attackAt=Time.time+(Stamina.Value<10?.75f:.38f); if(Pose)Pose.Attack=1; StartCoroutine(Strike(knife)); GameEvents.EmitNoise(transform.position,15);
        }
        IEnumerator Strike(bool knife)
        {
            yield return new WaitForSeconds(.12f); if(IsDead)yield break;
            int n=Physics.OverlapSphereNonAlloc(transform.position+Vector3.up+transform.forward*.8f, .9f,hits);int count=0;
            for(int i=0;i<n;i++) { var a=hits[i].GetComponentInParent<Actor>();if(!a||a==this||a.IsDead)continue; bool duplicate=false;for(int j=0;j<count;j++)if(victims[j]==a)duplicate=true;if(duplicate)continue;victims[count++]=a;a.TakeDamage(new DamageInfo(knife?(Session.Config&&Session.Config.knife?Session.Config.knife.damage:38):(Session.Config&&Session.Config.fists?Session.Config.fists.damage:15)*(combo==2?1.45f:1),knife?DamageType.Knife:DamageType.Punch,gameObject,transform.forward*2));Session.Hits++; }
            Session.RaiseAlert(.4f);
        }
        void PickUp()
        {
            if(held)return; int n=Physics.OverlapSphereNonAlloc(transform.position,1.8f,hits);
            for(int i=0;i<n;i++){var p=hits[i].GetComponent<Pickable>();if(!p)continue;if(p.knife){HasKnife=true;p.gameObject.SetActive(false);GameEvents.Notify("Navaja equipada · clic derecho");}else{held=p;p.Hold(Hand);GameEvents.Notify("Objeto recogido · clic izquierdo para lanzar");}return;}
        }
        public override void TakeDamage(DamageInfo d)
        {
            if(IsDead || State==MoveState.Boarded)return;float modifier=Session.Difficulty?Session.Difficulty.damage:1; Health=Mathf.Max(0,Health-d.amount*modifier);GameEvents.Hurt(d.amount);GameEvents.Hit(transform.position+Vector3.up);if(Health>0)return;State=MoveState.Dead;GameEvents.Die(d.type==DamageType.Bus?"Un bus te atropelló":d.type==DamageType.Fall?"Caída demasiado alta":"Te alcanzó la seguridad");
        }
        public void Board(Transform bus) { State=MoveState.Boarded;motor.enabled=false;transform.SetParent(bus,true);GameEvents.Notify("A bordo. Espera a que el bus arranque."); }
    }
}
