using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class CurveControlledBob : MonoBehaviour
//using System;
//using UnityEngine;


//namespace UnityStandardAssets.Utility
//{

public class CurveControlledBob : MonoBehaviour
{
    [Header("•à‚«ó‘Ô‚ÌƒJƒƒ‰‚Ì—h‚ê")]
        public float kHorizontalBobRange = 0.033f;
        public float kVerticalBobRange = 0.033f;
    [Header("Ž~‚Ü‚Á‚Ä‚¢‚éó‘Ô‚ÌƒJƒƒ‰‚Ì—h‚ê")]
    public float kStopHorizontalBobRange = 0.33f;
    public float kStopVerticalBobRange = 0.33f;
    [Header("‘–‚èó‘Ô‚ÌƒJƒƒ‰‚Ì—h‚ê")]
    public float kRunningHorizontalBobRange = 0.33f;
    public float kRunningVerticalBobRange = 0.33f;
    [Header("—h‚êó‘Ô‚ÌƒJƒƒ‰‚Ì—h‚ê")]
    public float kShakingHorizontalBobRange = 0.099f;
    public float kShakingVerticalBobRange = 0.099f;
    //[Header("“G‚ÉŒ©‚Â‚©‚Á‚½Žž‚ÌƒJƒƒ‰‚Ì—h‚ê")]
    //public float kContactHorizontalBobRange = 0.33f;
    //public float kContactVerticalBobRange = 0.33f;
    public AnimationCurve Bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                            new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                            new Keyframe(2f, 0f)); // sin curve for head bob
        public float VerticaltoHorizontalRatio = 1f;

        private float m_CyclePositionX;
        private float m_CyclePositionY;
        private float m_BobBaseInterval;
        private Vector3 m_OriginalCameraPosition;
        private float m_Time;


        public void Setup(Camera camera, float bobBaseInterval)
        {
            m_BobBaseInterval = bobBaseInterval;
            m_OriginalCameraPosition = camera.transform.localPosition;

            // get the length of the curve in time
            m_Time = Bobcurve[Bobcurve.length - 1].time;
        }

    public void UpdateOriginalCamPos(Camera camera)
    {
        m_OriginalCameraPosition = camera.transform.localPosition;
    }


    public Vector3 DoHeadBob(float speed, bool IsStop , bool IsRunning,bool IsShaking)
    {
        float HorizontalBobRange = kHorizontalBobRange;
        float VerticalBobRange = kVerticalBobRange;

        if (IsStop)
        {
            //Ž~‚Ü‚Á‚Ä‚¢‚é‚©”Û‚©‚ÅƒJƒƒ‰‚Ì—h‚ê‚ð•Ï‚¦‚é
            HorizontalBobRange = IsStop ? kStopHorizontalBobRange : kHorizontalBobRange;
            VerticalBobRange = IsStop ? kStopVerticalBobRange : kVerticalBobRange;
        }
        else if (IsRunning)
        {
            //‘–‚Á‚Ä‚¢‚é‚©”Û‚©‚ÅƒJƒƒ‰‚Ì—h‚ê‚ð•Ï‚¦‚é
            HorizontalBobRange = IsRunning ? kRunningHorizontalBobRange : kHorizontalBobRange;
            VerticalBobRange = IsRunning ? kRunningVerticalBobRange : kVerticalBobRange;
        }

        //—h‚êó‘Ô‚ð—Dæ
        if (IsShaking)
        {
            HorizontalBobRange = IsShaking ? kShakingHorizontalBobRange : kHorizontalBobRange;
            VerticalBobRange = IsShaking ? kShakingVerticalBobRange : kVerticalBobRange;
        }

        float xPos = /*m_OriginalCameraPosition.x*/ +(Bobcurve.Evaluate(m_CyclePositionX) * HorizontalBobRange);
        float yPos = /*m_OriginalCameraPosition.y*/ +(Bobcurve.Evaluate(m_CyclePositionY) * VerticalBobRange);

        m_CyclePositionX += (speed * Time.deltaTime) / m_BobBaseInterval;
        m_CyclePositionY += ((speed * Time.deltaTime) / m_BobBaseInterval) * VerticaltoHorizontalRatio;

        if (m_CyclePositionX > m_Time)
        {
            m_CyclePositionX = m_CyclePositionX - m_Time;
        }
        if (m_CyclePositionY > m_Time)
        {
            m_CyclePositionY = m_CyclePositionY - m_Time;
        }

        return new Vector3(xPos, yPos, 0f);
    }

}
