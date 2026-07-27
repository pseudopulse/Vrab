using System;
using R2API.Networking.Interfaces;

namespace Vrab {
    public class TargetTracker : HurtboxTracker {
        public DataMeter meter;
        public override void Start()
        {
            base.targetingIndicatorPrefab = Survivor.TargetPainter;
            base.maxSearchAngle = 30f;
            base.maxSearchDistance = 70f;
            base.targetType = TargetType.All;
            base.userIndex = TeamIndex.Player;
            base.Start();
            meter = GetComponent<DataMeter>();
        }
    }

    public class SyncVrabTarget : INetMessage
    {
        Transform target;
        GameObject tracker;
        public SyncVrabTarget(GameObject obj, Transform transform) {
            target = transform;
            tracker = obj;
        }
        public SyncVrabTarget() {

        }
        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            HurtBox box = reader.ReadHurtBoxReference().ResolveHurtBox();
            target = box != null ? box.transform : null;
            tracker = reader.ReadGameObject();
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            HurtBoxReference hb = HurtBoxReference.FromHurtBox(target.GetComponent<HurtBox>());
            writer.Write(hb);
            writer.Write(tracker);
        }

        void INetMessage.OnReceived()
        {
            tracker.GetComponent<TargetTracker>().target = target;
        }
    }
}