using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainR.PlayerManager;

namespace MainR
{
    public abstract class UIManager : MonoBehaviour
    {
        [SerializeField]
        protected GameObject pauseObj;

        public abstract void PauseReturn();
        public abstract void PauseQuit();
    }
}
