using System;
using UnityEngine;

namespace Headbob
{
    [Serializable]
    public abstract class BaseHeadbob
    {
        /// <summary>
        /// An event that gets triggered when the headbob hits its peak. Can be used to play footsteps efficiently.
        /// </summary>
        public Action OnHit;

        /// <summary>
        /// Initializes the headbob.
        /// </summary>
        public virtual void Initialize() {}

        /// <summary>
        /// Updates the headbob.
        /// </summary>
        public virtual void Update() {}
    }
}