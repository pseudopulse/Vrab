using System;

namespace Vrab.States {
    public class PelagicDrift : GenericCharacterMain {
        public float ascentSpeed = 10f;
        public float descentSpeed = 15f;
        public ParticleSystem vfx;
        public float timer = 0f;
        public DataMeter meter;
        public float y = 0f;

        public override void OnEnter()
        {
            base.OnEnter();

            vfx = FindModelChild("SwimVFX").GetComponent<ParticleSystem>();

            meter = GetComponent<DataMeter>();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!base.isAuthority) return;

            if (base.inputBank.jump.down && meter.Data > 0) {
                if (!vfx.isPlaying) {
                    vfx.Play();
                }
                if (base.characterMotor.isGrounded) {
                    timer = 1f;
                    base.characterMotor.Motor.ForceUnground();
                }
                meter.SpendData(10f * Time.fixedDeltaTime * Mathf.Clamp01(timer * 0.5f));
                timer += Time.fixedDeltaTime;
                base.characterMotor.velocity.y = Mathf.SmoothStep(y, ascentSpeed, Mathf.Clamp01(timer * 0.5f));
            }
            else {
                if (vfx.isPlaying) {
                    vfx.Stop();
                }
                
                timer = 0f;
                if (base.characterMotor.velocity.y < 0f) {
                    base.characterMotor.velocity += Vector3.up * descentSpeed * Time.fixedDeltaTime;
                }

                y = base.characterMotor.velocity.y;
            }
        }
    }
}