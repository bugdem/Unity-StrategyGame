using UnityEngine;
using UnityEngine.Events;

namespace GameEngine.Library.Utils
{
    [System.Serializable]
    public class ParticleEvent : UnityEvent<ParticleEventDispatcher> { }

    public class ParticleEventDispatcher : MonoBehaviour
    {
        public ParticleEvent OnParticleStopped;

        private void OnParticleSystemStopped()
        {
            OnParticleStopped?.Invoke(this);
        }
    }
}