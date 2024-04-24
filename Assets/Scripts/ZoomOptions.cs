using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ZoomOptions : MonoBehaviour
{
    private static float[] topRigDefault = { 3.5f, 3f };
    private static float[] middleRigDefault = { 2.5f, 4.5f };
    private static float[] bottomRigDefault = { -0.795f, 3f };

    public static CinemachineFreeLook cmOpts;
    
    public static void ChangeZoom(int zoom)
    {
        cmOpts.m_Orbits[0].m_Height = topRigDefault[0] * zoom;
        cmOpts.m_Orbits[0].m_Radius = topRigDefault[1] * zoom;

        cmOpts.m_Orbits[1].m_Height = middleRigDefault[0] * zoom;
        cmOpts.m_Orbits[1].m_Radius = middleRigDefault[1] * zoom;

        cmOpts.m_Orbits[2].m_Height = bottomRigDefault[0] * zoom;
        cmOpts.m_Orbits[2].m_Radius = bottomRigDefault[1] * zoom;
    }
}
