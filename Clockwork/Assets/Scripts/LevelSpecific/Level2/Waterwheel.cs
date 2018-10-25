using UnityEngine;
using XInputDotNetPure;

public class Waterwheel : MonoBehaviour
{
    [SerializeField]
    private Renderer m_water = null;
    [SerializeField]
    private ParticleSystem m_mist = null;

    [Range(0, 1)]
    public float lowerRaiseTarget = 1.0f;

    [Range(0, 1)]
    public float upperRaiseTarget = 0.0f;

    [SerializeField]
    private float m_raisedHeight = 2.0f;
    [SerializeField]
    private float m_WaterHeight = -2.0f;
    [SerializeField]
    private float m_lowerHeight = -5.0f;

    [SerializeField]
    private float m_upperRaisedHeight = 2.0f;
    [SerializeField]
    private float m_upperTeethHeight = 1.0f;

    [SerializeField]
    [Range(0f, 0.5f)]
    private float m_linearDamping = 0.1f;

    [SerializeField]
    [Range(0, 360)]
    private float m_rotationSpeed = 30.0f;
    [Range(0, 90)]
    [SerializeField]
    private float m_rotationAcceleration = 5.0f;

    [System.Serializable]
    public class Sound
    {
        public AudioSource[] sources = new AudioSource[0];

        private float[] pitch;
        private float[] volume;

        public void Init()
        {
            pitch = new float[sources.Length];
            volume = new float[sources.Length];

            for (int i = 0; i < sources.Length; i++)
            {
                pitch[i] = sources[i].pitch;
                volume[i] = sources[i].volume;
            }
        }

        public void SetIntensity(float intensity)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i].pitch = pitch[i] * intensity;
                sources[i].volume = volume[i] * intensity;
            }
        }
    }

    [SerializeField]
    private Sound m_lowerSound;
    [SerializeField]
    private Sound m_lowerShiftSound;
    [SerializeField]
    private Sound m_upperSound;
    [SerializeField]
    private Sound m_upperShiftSound;
    [SerializeField]
    private AudioSource m_thunkSound;

    [Header("Bones")]
    [SerializeField] private Transform m_lowerShaft = null;
    [SerializeField] private Transform m_lowerShaftSlide = null;
    [SerializeField] private Transform m_middleGear = null;
    [SerializeField] private Transform m_middleGearPos = null;
    [SerializeField] private Transform m_upperShaft = null;
    [SerializeField] private Transform m_upperShaftSlide = null;
    [SerializeField] private Transform m_rightGear = null;
    [SerializeField] private Transform m_bridgeMiddle = null;
    [SerializeField] private Transform m_bridgeMiddlePos = null;
    [SerializeField] private Transform m_bridgeLeft01 = null;
    [SerializeField] private Transform m_bridgeLeft02 = null;
    [SerializeField] private Transform m_bridgeRight01 = null;
    [SerializeField] private Transform m_bridgeRight02 = null;

    private float m_initialYLower;
    private float m_initialYUpper;
    private float m_lowerRotation = 0f;
    private float m_upperRotation = 0f;
    private float m_lowerAngVelocity = 0f;
    private float m_upperAngVelocity = 0f;
    private float m_lowerRaise;
    private float m_upperRaise;
    private float m_lowerRaiseVelocity = 0f;
    private float m_upperRaiseVelocity = 0f;
    private bool m_upperGearConnected = true;

    private void Awake()
    {
        m_lowerRaise = lowerRaiseTarget;
        m_upperRaise = upperRaiseTarget;

        m_initialYLower = m_lowerShaft.position.y;
        m_initialYUpper = m_upperShaft.position.y;

        m_lowerSound.Init();
        m_upperSound.Init();
        m_lowerShiftSound.Init();
        m_upperShiftSound.Init();
    }

    private void FixedUpdate()
    {
        SetVelocity(ref m_lowerRaiseVelocity, m_lowerRaise, lowerRaiseTarget);
        SetVelocity(ref m_upperRaiseVelocity, m_upperRaise, upperRaiseTarget);
    }

    private void LateUpdate()
    {
        // move lower shaft
        m_lowerRaise += m_lowerRaiseVelocity * Time.deltaTime;
        
        float lowHeight = Mathf.LerpUnclamped(m_lowerHeight, m_raisedHeight, m_lowerRaise);
        float waterFactor = 1f - Mathf.Clamp01(Mathf.InverseLerp(m_lowerHeight, m_WaterHeight, lowHeight));
        
        SetY(m_lowerShaft, lowHeight + m_initialYLower);

        // compute rotation
        m_lowerAngVelocity = Mathf.MoveTowards(m_lowerAngVelocity, waterFactor * m_rotationSpeed, m_rotationAcceleration * Time.deltaTime);
        m_lowerRotation -= m_lowerAngVelocity * Time.deltaTime;

        // check if upper shaft can mesh gears and rotate/move accordingly
        m_upperRaise += m_upperRaiseVelocity * Time.deltaTime;
        float upperHeight = m_upperRaisedHeight * m_upperRaise;
        SetY(m_upperShaft, upperHeight + lowHeight + m_initialYUpper);

        const int teeth = 10;
        const float toothAngle = 360f / teeth;
        float deltaAngle = (m_upperRotation - 3f) - m_lowerRotation;
        deltaAngle = ((deltaAngle + (0.5f * toothAngle)) % toothAngle) - (0.5f * toothAngle);

        if (upperHeight < m_upperTeethHeight && Mathf.Abs(deltaAngle) < 4.0f)
        {
            if (!m_upperGearConnected)
            {
                m_lowerAngVelocity *= 0.2f;
                m_upperGearConnected = true;
                m_thunkSound.Play();
            }
            m_upperAngVelocity = m_lowerAngVelocity;
            m_upperRotation -= deltaAngle * 2.5f * Time.deltaTime;
        }
        else
        {
            m_upperAngVelocity = Mathf.MoveTowards(m_upperAngVelocity, 0, m_rotationAcceleration * Time.deltaTime);
            m_upperGearConnected = false;
        }

        m_upperRotation -= m_upperAngVelocity * Time.deltaTime;

        // apply rotations
        Quaternion axis = Quaternion.AngleAxis(135, Vector3.up) * Quaternion.Euler(-90, 0, 0);
        Quaternion lowerRot = axis * Quaternion.AngleAxis(m_lowerRotation, Vector3.right);
        Quaternion middleRot = axis * Quaternion.AngleAxis(-m_lowerRotation, Vector3.right);
        Quaternion upperRot = axis * Quaternion.AngleAxis(m_upperRotation, Vector3.right);

        // apply constraints
        m_lowerShaft.rotation = lowerRot;
        m_upperShaft.rotation = upperRot;
        m_rightGear.rotation = Quaternion.AngleAxis(-m_upperRotation, Vector3.right);
        m_lowerShaftSlide.rotation = axis;
        m_upperShaftSlide.rotation = axis;

        m_middleGear.position = m_middleGearPos.position;
        m_middleGear.rotation = middleRot;

        m_bridgeMiddle.position = m_bridgeMiddlePos.position;

        // set sound effects
        float lowerVelocity = Mathf.Abs(m_lowerRaiseVelocity * 3.0f);
        float upperVelocity = Mathf.Abs(m_upperRaiseVelocity * 3.0f) + lowerVelocity;
        m_lowerShiftSound.SetIntensity(lowerVelocity);
        m_upperShiftSound.SetIntensity(upperVelocity);
        m_lowerSound.SetIntensity(Mathf.Abs(m_lowerAngVelocity / m_rotationSpeed));
        m_upperSound.SetIntensity(Mathf.Abs(m_upperAngVelocity / m_rotationSpeed));

        // contorller shake
        float shake = Mathf.Abs(upperVelocity) / 6.5f;
        GamePad.SetVibration(PlayerIndex.One, shake, shake);

        // set wake effects
        m_water.material.SetFloat("_Wake", waterFactor);

        ParticleSystem.EmissionModule emission = m_mist.emission;
        emission.rateOverTimeMultiplier = 25f * waterFactor;
    }

    private void SetVelocity(ref float velocity, float current, float target)
    {
        velocity += (target - current);
        velocity *= 1f - m_linearDamping;
    }

    private void LookAt(Transform constrained, Transform target)
    {
        Vector3 dir = target.position - constrained.position;
        Debug.DrawRay(constrained.position, constrained.parent.rotation * Quaternion.Euler(0, 0, -90) * Vector3.forward);
        constrained.rotation = Quaternion.LookRotation(dir, target.forward) * Quaternion.Euler(0, 0, -90);
    }

    private void SetY(Transform bone, float y)
    {
        Vector3 position = bone.position;
        position.y = y;
        bone.position = position;
    }
}
