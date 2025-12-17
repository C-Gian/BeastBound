using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // Chi dobbiamo seguire? (Il Player)
    public float smoothSpeed = 5f; // Quanto è morbido il movimento (0 = scatto, basso = ritardo)
    public Vector3 offset;         // La distanza fissa da mantenere

    void Start()
    {
        // Calcoliamo la distanza attuale tra camera e player all'inizio del gioco
        // Così non dobbiamo settarla a mano coi numeri
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calcoliamo dove la camera DOVREBBE essere
        Vector3 desiredPosition = target.position + offset;

        // 2. Usiamo Lerp (Linear Interpolation) per andarci dolcemente
        // Spostati dalla posizione attuale (transform.position) a quella desiderata (desiredPosition)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. Applichiamo la posizione
        transform.position = smoothedPosition;
    }
}