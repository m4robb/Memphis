using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace MxMGameplay
{

    public class KeeperInput : MonoBehaviour
    {
        float AcquireTimer = 100;

        [SerializeField]
        private MxMEventDefinition LurkDef = null;

        [SerializeField]
        private MxMEventDefinition Search01Def = null;

        [SerializeField]
        private MxMEventDefinition Point01Def = null;

        private MxMAnimator MMA;

        void Start()
        {
            MMA = GetComponentInChildren<MxMAnimator>();
        }

        public void Lurk()
        {

            if (LurkDef != null && AcquireTimer > 30)
            {
                AcquireTimer = 0;
                MMA.BeginEvent(LurkDef);
            }
        }


        public void Point01()
        {
            if (Point01Def != null)
            {
                MMA.BeginEvent(Point01Def);
            }
        }
        public void Search01()
        {
            if (LurkDef != null)
            {
                MMA.BeginEvent(Search01Def);
            }
        }

        // Update is called once per frame
        void Update()
        {
            AcquireTimer += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.J))
            {
                MMA.SetRequiredTag("Jogging");

            }


            if (Input.GetKeyDown(KeyCode.L))
            {
                MMA.BeginEvent(LurkDef);

            }
        }
    }
}
