using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Transmilleno
{
    public sealed class LevelWorld : MonoBehaviour
    {
        public NavMeshSurface Surface; public Volume Effects; public BusController Bus, SouthBus;
        public LevelDefinition Definition; public GameObject Station;
        public static LevelWorld Create(LevelDefinition level, AssetCatalog catalog, bool actors)
        {
            var world = new GameObject("Calle 45 · Security level " + level.index).AddComponent<LevelWorld>();
            world.Definition = level;
            world.Build(catalog, actors);
            return world;
        }
        void Build(AssetCatalog catalog, bool actors)
        {
            var t = transform;
            var concrete = WorldArt.Material("concrete", new Color(.48f, .5f, .46f));
            var dark = WorldArt.Material("metal", new Color(.12f, .17f, .18f));
            var cream = WorldArt.Material("cream", new Color(.88f, .82f, .65f));
            var red = WorldArt.Material("red", new Color(.75f, .12f, .11f));
            var road = WorldArt.Material("asphalt", new Color(.12f, .15f, .17f));
            WorldArt.Part("Road", t, new Vector3(0, -.28f, 85), new Vector3(60, .5f, 290), road);
            Station = WorldArt.Model(catalog, "station", t, Vector3.zero, Quaternion.identity);
            Station.name = "Calle 45 · original station at human scale";
            // The supplied model is the only station. Its original floor meshes supply physical ramps.
            foreach (var f in Station.GetComponentsInChildren<MeshFilter>())
            {
                if (f.name.Contains("Concrete") || f.name.Contains("Aggregate"))
                    f.gameObject.AddComponent<MeshCollider>().sharedMesh = f.sharedMesh;
            }
            AddStationCollision();
            for (int side = -1; side <= 1; side += 2)
            {
                WorldArt.Part("Sidewalk", t, new Vector3(side * 11, -.02f, 85), new Vector3(8, .28f, 285), concrete);
                for (int z = -45; z < 224; z += 6)
                    WorldArt.Part("Lane marking", t, new Vector3(side * 6.8f, .015f, z), new Vector3(.12f, .025f, 2), cream, false);
                for (int z = -25; z < 205; z += 22)
                {
                    float h = 10 + Mathf.Abs(z % 11);
                    var facade = new GameObject("City facade");
                    facade.transform.SetParent(t);
                    facade.transform.position = new Vector3(side * 22, 0, z);
                    WorldArt.Part("Masonry", facade.transform, new Vector3(0, h * .5f, 0), new Vector3(12, h, 18), concrete, false);
                    var glass = WorldArt.Material("glass", new Color(.2f, .3f, .34f));
                    for (int floor = 1; floor < h / 2.7f; floor++)
                        for (int w = 0; w < 5; w++)
                        {
                            WorldArt.Part("Window frame", facade.transform, new Vector3(-side * 6.02f, floor * 2.7f, -6 + w * 3), new Vector3(.10f, 1.8f, 1.6f), cream, false);
                            WorldArt.Part("Window", facade.transform, new Vector3(-side * 6.09f, floor * 2.7f, -6 + w * 3), new Vector3(.06f, 1.55f, 1.35f), glass, false);
                        }
                    StaticBatchingUtility.Combine(facade);
                    WorldArt.Part("Streetlight", t, new Vector3(side * 10, 3.3f, z), new Vector3(.12f, 6.6f, .12f), dark, false, true);
                    if (Definition.night)
                    {
                        var light = new GameObject("Street lamp").AddComponent<Light>();
                        light.transform.SetParent(t);
                        light.transform.position = new Vector3(side * 6, 5.8f, z);
                        light.type = LightType.Point;
                        light.range = 13;
                        light.intensity = 5;
                        light.shadows = LightShadows.None;
                    }
                }
            }
            // Additional security is placed inside the original corridor; no replacement architecture.
            for (int i = 0; i < Mathf.Max(0, Definition.index - 4); i++)
            {
                var bar = WorldArt.Part("Security barrier", t, new Vector3(i % 2 == 0 ? -.9f : .9f, 1.59f, 19 + i * 4), new Vector3(1.6f, 1.1f, .18f), red);
                bar.AddComponent<Climbable>().route = "Control adicional";
            }
            var sun = new GameObject("Bogotá sun").AddComponent<Light>();
            sun.transform.SetParent(t);
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(Definition.night ? 18 : 38, -34, 0);
            sun.color = Definition.night ? new Color(.38f, .53f, .8f) : new Color(1, .84f, .67f);
            sun.intensity = Definition.night ? .65f : 1.6f;
            sun.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Definition.night ? new Color(.2f, .27f, .37f) : new Color(.5f, .57f, .62f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = Definition.night ? new Color(.035f, .065f, .11f) : new Color(.59f, .66f, .69f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = Definition.rain ? .016f : .008f;
            var volume = new GameObject("Color and lens").AddComponent<Volume>();
            volume.transform.SetParent(t);
            Effects = volume;
            volume.isGlobal = true;
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var tone = volume.profile.Add<Tonemapping>();
            tone.mode.Override(TonemappingMode.ACES);
            var bloom = volume.profile.Add<Bloom>();
            bloom.intensity.Override(.22f);
            bloom.threshold.Override(1.2f);
            bloom.active = Session.Save?.bloom ?? true;
            var motion = volume.profile.Add<MotionBlur>();
            motion.intensity.Override(.25f);
            motion.active = Session.Save?.motionBlur ?? false;
            var vig = volume.profile.Add<Vignette>();
            vig.intensity.Override(.2f);
            if (Definition.rain)
            {
                var rain = new GameObject("Rain").AddComponent<ParticleSystem>();
                rain.transform.SetParent(t);
                rain.transform.position = new Vector3(0, 14, 0);
                var main = rain.main;
                main.startLifetime = 1.5f;
                main.startSpeed = 14;
                main.startSize = .035f;
                main.startColor = new Color(.65f, .78f, .85f, .4f);
                main.maxParticles = 1200;
                var shape = rain.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(35, 1, 65);
                rain.transform.rotation = Quaternion.Euler(90, 0, 0);
                var emission = rain.emission;
                emission.rateOverTime = 650;
                var rr = rain.GetComponent<ParticleSystemRenderer>();
                rr.sharedMaterial = WorldArt.Material("rain", new Color(.6f, .7f, .8f));
                rr.renderMode = ParticleSystemRenderMode.Billboard;
                main.startSize3D = true;
                main.startSizeX = .018f;
                main.startSizeY = .32f;
                main.startSizeZ = .018f;
            }

            Surface = gameObject.AddComponent<NavMeshSurface>();
            Surface.collectObjects = CollectObjects.Children;
            Surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            Surface.layerMask = 1 << 0;
            Surface.BuildNavMesh();
            if (!actors)
            {
                WorldArt.Model(catalog, "bus", t, StationLayout.Stop(false), Quaternion.identity);
                WorldArt.Model(catalog, "bus", t, StationLayout.Stop(true), Quaternion.Euler(0, 180, 0));
                return;
            }
            for (int i = 0; i < Definition.guards + Definition.police; i++)
            {
                bool police = i >= Definition.guards;
                var npc = SpawnActor(catalog, police ? "police" : "guard", new Vector3(i % 2 == 0 ? -1 : 1, StationLayout.Floor, 2 + i * 5));
                var guard = npc.AddComponent<GuardAgent>();
                guard.Police = police;
                guard.Pose = npc.GetComponentInChildren<PoseDriver>();
                Equip(guard, catalog);
            }
            int crowd = Mathf.RoundToInt(Definition.pedestrians * (Session.Save?.crowd ?? 1));
            for (int i = 0; i < crowd; i++)
            {
                var npc = SpawnActor(catalog, i % 2 == 0 ? "pedestrian" : "pedestrian2", new Vector3(i % 3 == 0 ? -10 : .8f, StationLayout.Floor, 6 + i * 3));
                npc.AddComponent<PedestrianAgent>();
            }
            for (int i = 0; i < 8; i++)
            {
                var prop = WorldArt.Model(catalog, i == 0 ? "knife" : i % 2 == 0 ? "stone" : "can", t, new Vector3(i % 2 == 0 ? -8 : 8, .4f, -4 + i * 3), Quaternion.identity);
                if (!prop)
                    continue;
                var rb = prop.AddComponent<Rigidbody>();
                rb.mass = .6f;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                prop.AddComponent<BoxCollider>().size = new Vector3(.28f, .2f, .28f);
                prop.AddComponent<Pickable>().knife = i == 0;
            }
            SpawnBus(catalog, false);
            SpawnBus(catalog, true);
        }
        BoxCollider Box(string name, Vector3 center, Vector3 size, bool climb = false, bool fence = false)
        {
            var g = new GameObject(name);
            g.transform.SetParent(transform, false);
            g.transform.localPosition = center;
            var b = g.AddComponent<BoxCollider>();
            b.size = size;
            if (climb)
            {
                var c = g.AddComponent<Climbable>();
                c.route = name;
                c.fence = fence;
                c.height = size.y;
                c.hasTopClearance = true;
            }
            return b;
        }
        void AddStationCollision()
        {
            // Collision proxies measured on the original rails, glazing and turnstiles.
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Reja de acceso Calle 45", new Vector3(side * 2.18f, 1.18f, -8), new Vector3(.12f, 1.65f, 19), true, true);
                Box("Pared del vestíbulo", new Vector3(side * 2.18f, 2.2f, 1.5f), new Vector3(.12f, 2.4f, 3));
                float[] starts = { 3.08f, 20.55f, 55.73f, 73.19f, 105.94f, 123.41f };
                foreach (float start in starts)
                {
                    if (start < 4)
                    {
                        Box("Ventana original", new Vector3(side * 2.15f, 2.1f, (start + 8.38f) * .5f), new Vector3(.12f, 2.14f, 8.38f - start));
                        Box("Ventana original", new Vector3(side * 2.15f, 2.1f, (10.38f + 15.67f) * .5f), new Vector3(.12f, 2.14f, 5.29f));
                    }
                    else
                        Box("Ventana original", new Vector3(side * 2.15f, 2.1f, start + 6.3f), new Vector3(.12f, 2.14f, 12.6f));
                }
                foreach (var interval in new[] { new Vector2(37, 52), new Vector2(88, 102), new Vector2(139, 188) })
                    Box("Reja original del andén", new Vector3(side * 2.18f, 1.63f, (interval.x + interval.y) / 2), new Vector3(.12f, 1.2f, interval.y - interval.x), true, true);
            }
            Box("Torniquetes Calle 45", new Vector3(.62f, 1.53f, .15f), new Vector3(2.4f, 1f, .62f), true);
            Box("Taquilla original", new Vector3(-1.5f, 2.1f, -2), new Vector3(1.1f, 2.15f, 3));
            // Exclude roof surfaces from navigation while keeping camera and jump collisions.
            foreach (var interval in new[] { new Vector2(-4, 36), new Vector2(50, 87), new Vector2(102, 138) })
            {
                var roof = Box("Cubierta original", new Vector3(0, 3.72f, (interval.x + interval.y) / 2), new Vector3(4.6f, .18f, interval.y - interval.x));
                var area = roof.gameObject.AddComponent<NavMeshModifier>();
                area.overrideArea = true;
                area.area = 1;
            }
        }
        void Equip(GuardAgent guard, AssetCatalog catalog)
        {
            var pose = guard.Pose;
            if (!pose)
                return;
            pose.Initialize();
            pose.Firearm = guard.Police;
            pose.Armed = true;
            if (guard.Police)
            {
                WorldArt.Model(catalog, "pistol", pose.WeaponSocket, new Vector3(0, -.06f, .17f), Quaternion.Euler(0, -90, 0));
                guard.Muzzle = pose.WeaponSocket;
            }
            else
                WorldArt.Part("Security baton", pose.WeaponSocket, new Vector3(0, -.16f, .04f), new Vector3(.038f, .42f, .038f), WorldArt.Material("metal", Color.gray), false, true);
        }
        bool reinforcement;
        public void Reinforce(AssetCatalog catalog)
        {
            if (reinforcement)
                return;
            reinforcement = true;
            var npc = SpawnActor(catalog, "police", new Vector3(1, StationLayout.Floor, 28));
            var guard = npc.AddComponent<GuardAgent>();
            guard.Police = true;
            guard.Pose = npc.GetComponentInChildren<PoseDriver>();
            Equip(guard, catalog);
            GameEvents.Notify("Llegaron refuerzos de policía");
        }
        GameObject SpawnActor(AssetCatalog catalog, string key, Vector3 p)
        {
            var g = new GameObject(key);
            g.transform.SetParent(transform);
            g.layer = 9;
            if (NavMesh.SamplePosition(p, out var hit, 2, NavMesh.AllAreas))
                p = hit.position;
            g.transform.position = p;
            WorldArt.Model(catalog, key, g.transform, Vector3.zero, Quaternion.identity);
            var c = g.AddComponent<CapsuleCollider>();
            c.center = Vector3.up * .85f;
            c.height = 1.7f;
            c.radius = .24f;
            var agent = g.AddComponent<NavMeshAgent>();
            agent.radius = .24f;
            agent.height = 1.75f;
            agent.stoppingDistance = 1.1f;
            agent.acceleration = 6;
            agent.angularSpeed = 240;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            return g;
        }
        void SpawnBus(AssetCatalog catalog, bool reverse)
        {
            var g = new GameObject(reverse ? "Sur · Norte a Sur" : "Norte · Sur a Norte");
            g.transform.SetParent(transform);
            g.layer = 10;
            WorldArt.Model(catalog, "bus", g.transform, Vector3.zero, Quaternion.identity);
            var c = g.AddComponent<BoxCollider>();
            c.center = new Vector3(0, 1.45f, 0);
            c.size = new Vector3(2.8f, 2.9f, 17.8f);
            var bus = g.AddComponent<BusController>();
            g.AddComponent<BusAudio>();
            bus.Reverse = reverse;
            bus.Stop = StationLayout.Stop(reverse);
            bus.BoardingPoint = StationLayout.Boarding(reverse);
            bus.PlatformDoors = new Transform[2];
            int k = 0;
            foreach (Transform child in Station.transform)
                if (child.name.StartsWith(reverse ? "SouthDoor" : "NorthDoor"))
                {
                    if (k >= 2)
                        break;
                    bus.PlatformDoors[k++] = child;
                    var panel = child.gameObject.AddComponent<BoxCollider>();
                    panel.center = new Vector3(reverse ? -2.13f : 2.13f, 2.05f, StationLayout.DoorZ + (child.name.EndsWith("A") ? -.47f : .47f));
                    panel.size = new Vector3(.12f, 2.1f, .93f);
                }
            var door = WorldArt.Part("Sliding bus door", g.transform, new Vector3(-1.45f, 1.6f, 1.5f), new Vector3(.07f, 2, 1.4f), WorldArt.Material("glass", Color.gray), false);
            bus.Door = door.transform;
            if (reverse)
                SouthBus = bus;
            else
                Bus = bus;
        }
    }
}
