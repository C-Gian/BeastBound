using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 10f;
    public Animator animator;

    // Riferimento alla Camera principale per capire dove guardiamo
    private Camera mainCamera;

    void Start()
    {
        if (GameManager.instance != null && GameManager.instance.isReturningFromBattle)
        {
            transform.position = GameManager.instance.nextSpawnPosition;

            GameManager.instance.isReturningFromBattle = false;
        }

        mainCamera = Camera.main;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // --- CALCOLO DIREZIONE BASATO SULLA CAMERA (Nuovo) ---

        // Prendiamo la direzione "davanti" e "destra" della telecamera
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        // Appiattiamo la Y a 0 (altrimenti se guardi in alto, il personaggio prova a volare)
        camForward.y = 0;
        camRight.y = 0;

        // Normalizziamo i vettori (li rendiamo lunghi 1)
        camForward.Normalize();
        camRight.Normalize();

        // La direzione finale è la somma dei vettori
        // (Avanti * InputVerticale) + (Destra * InputOrizzontale)
        Vector3 direction = (camForward * moveZ + camRight * moveX).normalized;

        // -----------------------------------------------------

        float speed = direction.magnitude;

        if (animator != null)
        {
            animator.SetBool("Grounded", true);
            animator.SetFloat("MoveSpeed", speed);
        }

        if (direction.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}