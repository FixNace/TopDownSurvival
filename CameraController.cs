using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Настройки слежения")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private Transform target;

    // Для тряски
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    void LateUpdate()
    {
        // ИСПРАВЛЕНИЕ: Если игрока нет, пытаемся его найти каждую секунду
        if (target == null)
        {
            FindPlayer();
            if (target == null) return; // Если все еще нет, ничего не делаем
        }

        // 1. Плавное слежение
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 2. Тряска камеры
        if (shakeDuration > 0)
        {
            transform.position = smoothedPosition + Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            transform.position = smoothedPosition;
        }
    }

    public void ShakeCamera(float magnitude, float duration)
    {
        shakeMagnitude = magnitude;
        shakeDuration = duration;
    }
}