using UnityEngine;

public static class UIPositionCalculator
{
    public static Vector3 CalcularPosicion(Vector3 posicionMueble, Vector3 camaraRight, float distanciaAlObjeto, float alturaOffset)
    {
        Vector3 direccionIzquierda = -camaraRight;

        return posicionMueble
            + (direccionIzquierda * distanciaAlObjeto)
            + (Vector3.up * alturaOffset);
    }
}