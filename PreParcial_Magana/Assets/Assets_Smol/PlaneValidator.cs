using UnityEngine;

public static class PlaneValidator
{
    public static bool EsPlanoHorizontal(Pose hitPose, float maxAnguloPermitido = 25f)
    {
        float anguloConVertical = Vector3.Angle(hitPose.up, Vector3.up);

        return anguloConVertical <= maxAnguloPermitido;
    }
}