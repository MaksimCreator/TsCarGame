using UnityEngine;

namespace Ashsvp
{
    [RequireComponent(typeof(ParticleSystem))]
    public class TireSmoke : MonoBehaviour
    {
        private ParticleSystem smoke;
        private void Awake()
        {
            smoke = GetComponent<ParticleSystem>();
            smoke.Stop();
        }

        public void playSmoke()
        {
            smoke.Play();
        }

        public void stopSmoke()
        {
            smoke.Stop();
        }
    }
}
