using System;

namespace Vrab.States {
    public class PelagicDrift : GenericCharacterMain {
        public float ascentSpeed = 10f;
        public float descentSpeed = 15f;
        public ParticleSystem vfx;
        public float timer = 0f;
        public DataMeter meter;
        public float y = 0f;
        public float timeSinceGrounded = 0f;
        public Animator anim;
        public bool requireTap = false;

        public override void OnEnter()
        {
            base.OnEnter();

            vfx = FindModelChild("SwimVFX").GetComponent<ParticleSystem>();

            meter = GetComponent<DataMeter>();
            anim = GetModelAnimator();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!base.isAuthority) return;

            if (base.characterMotor.isGrounded) {
                timer = 1f;
                timeSinceGrounded = 0f;
                requireTap = true;

                if (requireTap && base.inputBank.jump.justPressed) {
                    base.characterMotor.Motor.ForceUnground();
                }
            }

            if ((!requireTap ? base.inputBank.jump.down : base.inputBank.jump.justPressed) && (meter.Data > 0 || timeSinceGrounded <= 0.7f)) {
                if (!vfx.isPlaying) {
                    vfx.Play();
                }

                requireTap = false;

                timeSinceGrounded += Time.fixedDeltaTime;
                if (timeSinceGrounded >= 1f) {
                    meter.SpendData(7f * Time.fixedDeltaTime * Mathf.Clamp01(timer * 0.5f));
                    meter.drifting = true;
                }

                timer += Time.fixedDeltaTime;
                base.characterMotor.velocity.y = Mathf.SmoothStep(y, ascentSpeed * Math.Max(4f - timeSinceGrounded * 5f, 1f), Mathf.Clamp01(timer * 0.5f));
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
                meter.drifting = false;
            }
        }
    }
}