using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Transmilleno
{
    public sealed class LevelWorld : MonoBehaviour
    {
        public NavMeshSurface Surface; public Volume Effects; public BusController Bus; public LevelDefinition Definition;
        public static LevelWorld Create(LevelDefinition level, AssetCatalog catalog, bool actors)
        {
            var root = new GameObject("Station · " + level.station);
            var world = root.AddComponent<LevelWorld>();
            world.Definition = level;
            world.Build(catalog, actors);
            return world;
        }
        void Build(AssetCatalog catalog, bool actors)
        {
            var t = transform;
            var concrete = WorldArt.Material("concrete", new Color(.48f, .5f, .46f));
            var dark = WorldArt.Material("metal", new Color(.12f, .17f, .18f));
            var red = WorldArt.Material("red", new Color(.75f, .12f, .11f));
            var cream = WorldArt.Material("cream", new Color(.88f, .82f, .65f));
            var yellow = WorldArt.Material("yellow", new Color(.95f, .63f, .14f));
            var glass = WorldArt.Material("glass", new Color(.25f, .45f, .49f));
            var road = WorldArt.Material("asphalt", new Color(.12f, .15f, .17f));
            WorldArt.Part("Bogotá asphalt", t, new Vector3(0, -.28f, 0), new Vector3(70, .5f, 140), road);
            for (int side = -1; side <= 1; side += 2)
            {
                WorldArt.Part("Sidewalk kerb", t, new Vector3(side * 13, -.02f, 0), new Vector3(8, .28f, 125), concrete);
                for (int z = -58; z < 60; z += 3)
                {
                    WorldArt.Part("Lane marking", t, new Vector3(side * 8.3f, .015f, z), new Vector3(.12f, .025f, 1.4f), cream, false);
                    WorldArt.Part("Pavement seam", t, new Vector3(side * 13, .13f, z), new Vector3(7.8f, .013f, .025f), dark, false);
                }
            }
            WorldArt.Part("Raised platform", t, new Vector3(0, .17f, 1), new Vector3(6.4f, .4f, 40), concrete);
            for (int z = -18; z < 21; z++)
            {
                WorldArt.Part("Tactile paving", t, new Vector3(2.88f, .39f, z), new Vector3(.3f, .035f, .92f), yellow, false);
                WorldArt.Part("Tile joint", t, new Vector3(0, .382f, z), new Vector3(6, .015f, .022f), dark, false);
            }
            // The user's station is preserved as the opposite, non-traversable platform with original architecture.
            var station = WorldArt.Model(catalog, Definition.index % 2 == 0 ? "stationSabana" : "station", t, new Vector3(22, 0, 5), Quaternion.identity);
            if (station)
                station.name = "Original architecture · supplied by user";
            for (int z = -15; z <= 18; z += 6)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    WorldArt.Part("Steel column", t, new Vector3(side * 2.75f, 2.2f, z), new Vector3(.18f, 3.6f, .18f), dark);
                    WorldArt.Part("Footing", t, new Vector3(side * 2.75f, .48f, z), new Vector3(.42f, .16f, .42f), dark);
                }
                WorldArt.Part("Roof spine", t, new Vector3(0, 4.04f, z), new Vector3(6.7f, .16f, .25f), dark);
                var roof = WorldArt.Part("Corrugated canopy", t, new Vector3(0, 4.25f, z), new Vector3(7, .16f, 5.9f), cream);
                var roofArea = roof.AddComponent<NavMeshModifier>();
                roofArea.overrideArea = true;
                roofArea.area = 1;
                WorldArt.Part("Roof red fascia", t, new Vector3(-3.5f, 4.12f, z), new Vector3(.12f, .36f, 5.9f), red);
                for (int k = 0; k < 5; k++)
                    WorldArt.Part("Roof rib", t, new Vector3(0, 4.38f, z - 2 + k), new Vector3(7, .04f, .045f), dark, false);
                WorldArt.Part("Strip light", t, new Vector3(0, 3.91f, z), new Vector3(2, .04f, .16f), cream, false);
                if (levelNight())
                {
                    var light = new GameObject("Platform light").AddComponent<Light>();
                    light.transform.SetParent(t);
                    light.transform.localPosition = new Vector3(0, 3.6f, z);
                    light.type = LightType.Point;
                    light.range = 8;
                    light.intensity = 12;
                    light.color = new Color(1, .74f, .46f);
                }
            }
            // Two turnstiles, a medium gate and fences provide independent entry routes.
            WorldArt.Part("Entry walkway", t, new Vector3(-6, .14f, -15), new Vector3(7, .35f, 3.5f), concrete);
            for (int z = -16; z <= -14; z += 2)
            {
                var turn = new GameObject("Turnstile · vault route");
                turn.transform.SetParent(t);
                turn.transform.localPosition = new Vector3(-4.4f, 0, z);
                var body = WorldArt.Part("Stainless cabinet", turn.transform, new Vector3(0, .77f, 0), new Vector3(.7f, 1.05f, .52f), dark);
                body.AddComponent<Climbable>().route = "Torniquete";
                WorldArt.Part("Validator", turn.transform, new Vector3(0, 1.34f, 0), new Vector3(.3f, .14f, .3f), red, false);
                WorldArt.Part("Display", turn.transform, new Vector3(-.17f, 1.32f, 0), new Vector3(.018f, .07f, .17f), glass, false);
                for (int k = 0; k < 3; k++)
                {
                    var arm = WorldArt.Part("Tripod arm", turn.transform, new Vector3(0, .85f, .5f), new Vector3(.08f, .08f, 1.1f), cream, false, true);
                    arm.transform.localRotation = Quaternion.Euler(k * 120, 90, 0);
                }
            }
            for (int z = -18; z < 20; z += 3)
            {
                if (z == -15)
                    continue;
                float h = Definition.index >= 3 && z > 8 ? 3.3f : 2.6f;
                var frame = new GameObject("Fence panel");
                frame.transform.SetParent(t);
                frame.transform.localPosition = new Vector3(-3.25f, 0, z);
                var box = frame.AddComponent<BoxCollider>();
                box.center = new Vector3(0, h * .5f, 0);
                box.size = new Vector3(.16f, h, 2.9f);
                var climb = frame.AddComponent<Climbable>();
                climb.fence = true;
                climb.height = h;
                climb.route = "Reja";
                climb.hasTopClearance = z < 0;
                for (int j = 0; j < 9; j++)
                    WorldArt.Part("Vertical bars", frame.transform, new Vector3(0, h * .5f, -1.4f + j * .35f), new Vector3(.045f, h, .045f), dark, false);
                WorldArt.Part("Crossbeam", frame.transform, new Vector3(0, h - .15f, 0), new Vector3(.08f, .1f, 2.95f), dark, false);
                WorldArt.Part("Crossbeam lower", frame.transform, new Vector3(0, .4f, 0), new Vector3(.08f, .1f, 2.95f), dark, false);
                if (!climb.hasTopClearance)
                    WorldArt.Part("Covered fence top", frame.transform, new Vector3(0, h + .12f, 0), new Vector3(1.5f, .2f, 3), cream);
            }
            var gate = WorldArt.Part("Access gate", t, new Vector3(-6, 1.05f, 12 + Definition.layout), new Vector3(.2f, 2.1f, 2.6f), red);
            gate.AddComponent<Climbable>().route = "Puerta";
            WorldArt.Part("Gate viewing slit", t, new Vector3(-6.13f, 1.5f, 12 + Definition.layout), new Vector3(.04f, .5f, 1.8f), glass, false);
            var edge = WorldArt.Part("Low track barrier", t, new Vector3(3.2f, .65f, -9), new Vector3(.3f, 1.1f, 5), dark);
            edge.AddComponent<Climbable>().route = "Cruce de vía";
            WorldArt.Part("Station sign board", t, new Vector3(-3.64f, 3.7f, -9), new Vector3(.1f, .74f, 5.1f), red, false);
            WorldArt.Sign("TRANSMILLENO  /  " + Definition.station.ToUpperInvariant(), t, new Vector3(-3.71f, 3.7f, -9), 1.8f, Color.white, 90);
            WorldArt.Sign(Definition.station.ToUpperInvariant(), t, new Vector3(0, 3.36f, -15), 2.6f, Color.white, 0);
            WorldArt.Sign("01    →    NORTE", t, new Vector3(2.9f, 2.8f, 7), 1.4f, Color.white, -90);
            for (int z = -10; z < 18; z += 9)
            {
                WorldArt.Part("Bench slatted seat", t, new Vector3(-1.6f, .85f, z), new Vector3(.6f, .13f, 2.1f), cream);
                for (int k = 0; k < 4; k++)
                    WorldArt.Part("Bench slat", t, new Vector3(-1.87f, 1.13f, z - .78f + k * .51f), new Vector3(.12f, .65f, .45f), cream, false);
                for (int side = -1; side <= 1; side += 2)
                    WorldArt.Part("Bench foot", t, new Vector3(-1.6f, .59f, z + side * .8f), new Vector3(.4f, .6f, .13f), dark);
            }
            for (int z = -36; z < 50; z += 12)
            {
                var post = WorldArt.Part("Streetlight", t, new Vector3(-15, 3.3f, z), new Vector3(.16f, 6.6f, .16f), dark, false, true);
                WorldArt.Part("Lamp arm", t, new Vector3(-14, 6.4f, z), new Vector3(2.1f, .13f, .15f), dark, false);
                WorldArt.Part("Lamp", t, new Vector3(-13, 6.3f, z), new Vector3(.7f, .15f, .4f), cream, false);
            }
            for (int z = -48; z < 55; z += 11)
            {
                float height = 8 + (Mathf.Abs(z * 13 + Definition.index * 7) % 13);
                var buildingMat = WorldArt.Material("building" + (z % 3), Color.Lerp(new Color(.37f, .32f, .27f), new Color(.54f, .59f, .56f), Mathf.Abs(z % 3) * .4f));
                WorldArt.Part("City facade", t, new Vector3(-25, height * .5f, z), new Vector3(10, height, 9.5f), buildingMat, false);
                for (int floor = 1; floor < height / 2.5f; floor++)
                    for (int w = 0; w < 3; w++)
                    {
                        WorldArt.Part("Window frame", t, new Vector3(-19.95f, floor * 2.5f, z - 3 + w * 3), new Vector3(.16f, 1.7f, 1.7f), cream, false);
                        WorldArt.Part("Window", t, new Vector3(-19.84f, floor * 2.5f, z - 3 + w * 3), new Vector3(.1f, 1.45f, 1.45f), glass, false);
                    }
            }
            // Street stall, route map, litter bins and graffiti anchor the station to the city.
            WorldArt.Part("Vendor counter", t, new Vector3(-13, .85f, -6), new Vector3(2, 1.3f, 1.2f), red);
            WorldArt.Part("Vendor canopy", t, new Vector3(-13, 2.6f, -6), new Vector3(2.5f, .18f, 1.8f), yellow);
            for (int s = -1; s <= 1; s += 2)
                WorldArt.Part("Canopy pole", t, new Vector3(-13 + s * .85f, 1.7f, -6), new Vector3(.07f, 2, .07f), dark, false);
            WorldArt.Sign("TINTO  /  AREPA", t, new Vector3(-13, 1.1f, -6.63f), 1.2f, cream.color, 0);
            WorldArt.Sign("BOGOTÁ\nSE MUEVE", t, new Vector3(-19.7f, 2, -24), 4, red.color, -90);
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
            AddVariant(catalog, concrete, dark, red, cream, glass);
            Surface = rootSurface();
            Surface.BuildNavMesh();
            if (!actors)
            {
                var display = WorldArt.Model(catalog, "bus", t, new Vector3(5.1f, 0, 4), Quaternion.identity);
                if (display)
                    display.AddComponent<MenuTraffic>();
                return;
            }
            for (int i = 0; i < Definition.guards + Definition.police; i++)
            {
                bool police = i >= Definition.guards;
                var npc = SpawnActor(catalog, police ? "police" : "guard", new Vector3(i % 2 == 0 ? 0 : -10, .4f, -4 + i * 4));
                var a = npc.AddComponent<GuardAgent>();
                a.Police = police;
                a.Pose = npc.GetComponentInChildren<PoseDriver>();
                if (a.Pose && a.Pose.rightForearm)
                {
                    if (police)
                    {
                        var pistol = WorldArt.Model(catalog, "pistol", a.Pose.rightForearm, new Vector3(-.15f, 0, 0), Quaternion.Euler(0, 90, 0));
                    }
                    else
                    {
                        WorldArt.Part("Baton grip", a.Pose.rightForearm, new Vector3(-.2f, 0, 0), new Vector3(.05f, .12f, .05f), dark, false, true);
                        WorldArt.Part("Security baton", a.Pose.rightForearm, new Vector3(-.2f, -.2f, 0), new Vector3(.042f, .42f, .042f), dark, false, true);
                    }
                }
            }
            int crowd = Mathf.RoundToInt(Definition.pedestrians * (Session.Save?.crowd ?? 1));
            for (int i = 0; i < crowd; i++)
            {
                var npc = SpawnActor(catalog, i % 2 == 0 ? "pedestrian" : "pedestrian2", new Vector3(i % 2 == 0 ? -12 : 1, .4f, -14 + (i * 3) % 30));
                npc.AddComponent<PedestrianAgent>();
            }
            for (int i = 0; i < 12; i++)
            {
                var prop = WorldArt.Model(catalog, i == 0 ? "knife" : i % 2 == 0 ? "stone" : "can", t, new Vector3(-10 + (i % 3) * 1.4f, .45f, -12 + i * 2.4f), Quaternion.identity);
                if (!prop)
                    continue;
                var rb = prop.AddComponent<Rigidbody>();
                rb.mass = .6f;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                var col = prop.AddComponent<BoxCollider>();
                col.size = new Vector3(.28f, .2f, .28f);
                var pick = prop.AddComponent<Pickable>();
                pick.knife = i == 0;
            }
            SpawnBus(catalog, false);
            if (Definition.index >= 4)
                SpawnBus(catalog, true);
        }

        void AddVariant(AssetCatalog catalog, Material concrete, Material dark, Material red, Material cream, Material glass)
        {
            int level = Definition.index;
            var t = transform;
            for (int i = 0; i < level % 4; i++)
            {
                float z = -7 + i * 8 + (level % 2) * 2;
                WorldArt.Part("Information kiosk", t, new Vector3(.3f, .9f, z), new Vector3(1.1f, 1.1f, .7f), dark);
                WorldArt.Part("Route map glass", t, new Vector3(.3f, 1.5f, z - .38f), new Vector3(.96f, 1.3f, .05f), glass, false);
                WorldArt.Sign("RUTAS / " + (level + i).ToString("00") + "\nNORTE ↑", t, new Vector3(.3f, 1.55f, z - .42f), .8f, Color.white, 0);
            }
            if (level >= 3)
            {
                float z = 10 - level * .9f;
                var booth = new GameObject("Ticket hall");
                booth.transform.SetParent(t);
                booth.transform.localPosition = new Vector3(-11, 0, z);
                WorldArt.Part("Ticket office", booth.transform, new Vector3(0, 1.2f, 0), new Vector3(3, 2.4f, 2), cream);
                WorldArt.Part("Window counter", booth.transform, new Vector3(0, 1.45f, -1.02f), new Vector3(2, .8f, .06f), glass, false);
                WorldArt.Part("Red awning", booth.transform, new Vector3(0, 2.48f, -.3f), new Vector3(3.5f, .18f, 2.7f), red);
                WorldArt.Sign("TAQUILLA", booth.transform, new Vector3(0, 2.08f, -1.07f), 1, Color.white, 0);
            }
            if (level >= 5)
            {
                float z = 18 + (level % 3) * 2;
                for (int i = 0; i < 18; i++)
                    WorldArt.Part("Footbridge stair", t, new Vector3(-13 + i * .42f, (i + 1) * .135f, z), new Vector3(.44f, (i + 1) * .27f, 2.2f), concrete);
                WorldArt.Part("Elevated pedestrian bridge", t, new Vector3(-2.8f, 4.81f, z), new Vector3(13.5f, .25f, 2.4f), concrete);
                for (int side = -1; side <= 1; side += 2)
                {
                    WorldArt.Part("Bridge rail", t, new Vector3(-2.8f, 5.8f, z + side * 1.13f), new Vector3(13.5f, .08f, .08f), dark);
                    for (int x = -9; x < 4; x++)
                        WorldArt.Part("Bridge baluster", t, new Vector3(x, 5.33f, z + side * 1.13f), new Vector3(.065f, 1, .065f), dark, false);
                }
                for (int x = -7; x < 6; x += 10)
                    WorldArt.Part("Bridge structural support", t, new Vector3(x, 2.4f, z), new Vector3(.35f, 4.8f, .35f), dark);
                WorldArt.Sign("PASARELA  /  ACCESO NORTE", t, new Vector3(-2.8f, 5.25f, z - 1.26f), 1.1f, Color.white, 0);
            }
            if (level >= 7)
            {
                for (int i = 0; i < level - 5; i++)
                {
                    float z = -12 + i * 4.2f;
                    var barrier = WorldArt.Part("Security chicane", t, new Vector3(i % 2 == 0 ? -.4f : 1.3f, .9f, z), new Vector3(1.7f, 1.1f, .22f), red);
                    barrier.AddComponent<Climbable>().route = "Barreras interiores";
                    WorldArt.Part("Warning stripe", t, new Vector3(i % 2 == 0 ? -.4f : 1.3f, 1.12f, z - .13f), new Vector3(1.5f, .14f, .025f), cream, false);
                }
            }
            for (int i = 0; i < 3; i++)
            {
                float z = -25 + i * 24;
                WorldArt.Part("Tree planter", t, new Vector3(-16, .45f, z), new Vector3(1.4f, .7f, 1.4f), concrete);
                WorldArt.Part("Tree trunk", t, new Vector3(-16, 1.8f, z), new Vector3(.18f, 2.6f, .18f), dark, false, true);
                for (int j = 0; j < 4; j++)
                {
                    var foliage = WorldArt.Part("Pruned canopy", t, new Vector3(-16 + (j % 2) * .6f - .3f, 3.4f + j * .25f, z + (j / 2) * .6f - .3f), new Vector3(1.6f, 1.1f, 1.5f), WorldArt.Material("leaves", new Color(.22f, .36f, .23f)), false);
                    foliage.transform.Rotate(0, j * 35, 0);
                }
            }
        }
        bool reinforcement;
        public void Reinforce(AssetCatalog catalog)
        {
            if (reinforcement)
                return;
            reinforcement = true;
            var npc = SpawnActor(catalog, "police", new Vector3(-11, .4f, 15));
            var guard = npc.AddComponent<GuardAgent>();
            guard.Police = true;
            guard.Pose = npc.GetComponentInChildren<PoseDriver>();
            GameEvents.Notify("Llegaron refuerzos de policía");
        }
        bool levelNight() => Definition.night;
        NavMeshSurface rootSurface()
        {
            var s = gameObject.AddComponent<NavMeshSurface>();
            s.collectObjects = CollectObjects.Children;
            s.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            s.layerMask = 1 << 0;
            return s;
        }
        GameObject SpawnActor(AssetCatalog catalog, string key, Vector3 p)
        {
            var g = new GameObject(key);
            g.transform.SetParent(transform);
            if (NavMesh.SamplePosition(p, out var hit, 1, NavMesh.AllAreas))
                p = hit.position;
            else if (NavMesh.SamplePosition(new Vector3(0, .4f, 5), out hit, 1, NavMesh.AllAreas))
                p = hit.position;
            g.transform.position = p;
            g.layer = 9;
            WorldArt.Model(catalog, key, g.transform, Vector3.zero, Quaternion.identity);
            var c = g.AddComponent<CapsuleCollider>();
            c.center = Vector3.up * .85f;
            c.height = 1.7f;
            c.radius = .28f;
            var agent = g.AddComponent<NavMeshAgent>();
            agent.radius = .3f;
            agent.height = 1.75f;
            agent.stoppingDistance = 1.25f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            return g;
        }
        void SpawnBus(AssetCatalog catalog, bool reverse)
        {
            var g = new GameObject(reverse ? "Express bus" : "B74 · Boarding bus");
            g.transform.SetParent(transform);
            g.layer = 10;
            var model = WorldArt.Model(catalog, "bus", g.transform, Vector3.zero, Quaternion.identity);
            var c = g.AddComponent<BoxCollider>();
            c.center = new Vector3(0, 1.45f, 0);
            c.size = new Vector3(2.8f, 2.9f, 17.8f);
            var bus = g.AddComponent<BusController>();
            g.AddComponent<BusAudio>();
            bus.Reverse = reverse;
            bus.Stop = new Vector3(reverse ? -6 : 5.1f, 0, 4);
            var door = WorldArt.Part("Animated door", g.transform, new Vector3(-1.45f, 1.6f, 1.5f), new Vector3(.07f, 2, 1.4f), WorldArt.Material("glass", Color.gray), false);
            bus.Door = door.transform;
            if (!reverse)
                Bus = bus;
        }
    }
}
