using UnityEngine;

public class AutoWheelSpinner : MonoBehaviour
{
    [Header("Speed Settings")]
    public float minMaxSpeed = 200f;
    public float maxMaxSpeed = 900f;

    [Header("Feel Settings")]
    public float startAcceleration = 40f;
    public float deceleration = 120f;

    private float currentSpeed = 0f;
    private float targetMaxSpeed;
    private float currentAcceleration;
    private float timer = 0f;
    private float currentTargetDuration;
    private bool isSpinning = false;

    private float spinDirection = 1f;

    void Start()
    {
        SetupRandomSpin();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isSpinning)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetMaxSpeed, currentAcceleration * Time.deltaTime);

            if (timer >= currentTargetDuration)
            {
                isSpinning = false;
                timer = 0;
                currentTargetDuration = Random.Range(1.5f, 3f);
            }
        }
        else
        {

            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);

            if (timer >= currentTargetDuration && currentSpeed <= 0)
            {
                SetupRandomSpin();
            }
        }

        float rotationStep = spinDirection * currentSpeed * Time.deltaTime;
        transform.Rotate(0, 0, rotationStep);
    }

    void SetupRandomSpin()
    {
        isSpinning = true;
        timer = 0;

        spinDirection = (Random.value > 0.5f) ? 1f : -1f;

        targetMaxSpeed = Random.Range(minMaxSpeed, maxMaxSpeed);

        currentAcceleration = Random.Range(startAcceleration, startAcceleration * 4f);

        currentTargetDuration = Random.Range(2f, 7f);
    }
}