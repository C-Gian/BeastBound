using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;          // Il Player da seguire
    public float mouseSensitivity = 3.0f;
    public float distanceFromTarget = 5.0f; // Quanto è lontana la camera

    // Limiti verticali (per non finire sotto terra o rompersi il collo)
    public float pitchMin = -10f;
    public float pitchMax = 60f;

    // Offset per mirare alla testa/spalle e non ai piedi
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0);

    // Variabili private per tenere traccia della rotazione
    private float yaw = 0.0f;   // Rotazione orizzontale
    private float pitch = 0.0f; // Rotazione verticale

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // --- SOLUZIONE "FIDATI DEI MIEI VALORI" ---

        // 1. Leggiamo la rotazione iniziale che hai impostato nell'Inspector
        Vector3 angles = transform.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;

        // 2. Se c'è un target, calcoliamo la distanza iniziale esatta
        if (target != null)
        {
            // Il punto che la camera dovrebbe guardare
            Vector3 targetPoint = target.position + targetOffset;

            // La distanza tra dove hai messo la camera e quel punto
            distanceFromTarget = Vector3.Distance(transform.position, targetPoint);

            // Opzionale: stampiamo la distanza calcolata nella console per verifica
            Debug.Log("Distanza iniziale calcolata: " + distanceFromTarget);
        }
        // ------------------------------------------
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Leggiamo l'input del mouse
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 2. Blocchiamo la rotazione verticale (Clamp)
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // 3. Calcoliamo la rotazione (Matematica dei Quaterni)
        // Eulero: Pitch (X), Yaw (Y), Roll (Z)
        Vector3 targetRotation = new Vector3(pitch, yaw, 0);
        transform.eulerAngles = targetRotation;

        // 4. Calcoliamo la posizione
        // La posizione è: PosizioneTarget + Offset - (DirezioneSguardo * Distanza)
        Vector3 finalPosition = target.position + targetOffset - (transform.forward * distanceFromTarget);

        transform.position = finalPosition;
    }
}