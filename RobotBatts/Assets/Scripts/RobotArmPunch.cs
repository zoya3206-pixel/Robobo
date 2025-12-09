using System.Collections;
using UnityEngine;
using Bhaptics.SDK2;
using Unity.XR.CoreUtils;

public class RobotArmPunchController : MonoBehaviour
{
    [Header("Robot Arm References")]
    [SerializeField] public Transform LeftBicep;
    [SerializeField] public Transform LeftForearm;
    [SerializeField] public Transform RightBicep;
    [SerializeField] public Transform RightForearm;

    public bool IsLeftArmPunching = false;
    public bool IsRightArmPunching = false;

    private Vector3 m_forearm_idle_pos;
    private Quaternion m_forearm_idle_rot;
    private Vector3 m_forearm_hit_pos;
    private Quaternion m_forearm_hit_rot;

    private Vector3 m_bicep_idle_pos;
    private Quaternion m_bicep_idle_rot;
    private Quaternion m_bicep_hit_rot;
    private Vector3 m_bicep_hit_pos;

    private bool m_rcan_unpunching = false;
    private bool m_lcan_unpunching = false;

    private float m_rtime_count = 0.0f;
    private float m_ltime_count = 0.0f;

    private void Start()
    {
        m_forearm_idle_pos = new Vector3(63.2700005f, 83.1303101f, -61.6399994f);
        m_forearm_idle_rot = new Quaternion(0.680873752f, -0.355771303f, 0.160510227f, 0.619737208f);
        m_forearm_hit_pos = new Vector3(65.2600021f, 61.2999992f, -45.0099983f);
        m_forearm_hit_rot = new Quaternion(0.383888751f, -0.264530629f, 0.28698501f, 0.836834967f);

        m_bicep_idle_pos = new Vector3(1.49691522f, 2.73115563f, 2.0158751f);
        m_bicep_idle_rot = new Quaternion(0.0343496725f, -0.224308044f, 0.00791161321f, 0.973880649f);
        m_bicep_hit_pos = new Vector3(8.72745323f, 33.3327637f, -76.2455902f);
        m_bicep_hit_rot = new Quaternion(0.434348106f, -0.259771347f, -0.0657111406f, 0.859966695f);
    }

    private void Update()
    {
        if (IsRightArmPunching)
        {
            RightArmPunching();
            RightArmUnPunching();
        }
        if (IsLeftArmPunching)
        {
            LeftArmPunching();
            LeftArmUnPunching();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftController"))
        {
            if (gameObject.CompareTag("Hit") && !IsLeftArmPunching)
            {
                IsLeftArmPunching = true;
            }
        }
        else if (other.CompareTag("RightController"))
        {
            if (gameObject.CompareTag("Hit") && !IsRightArmPunching)
            {
                IsRightArmPunching = true;
            }
        }
    }

    private void RightArmPunching()
    {
        if (m_rcan_unpunching) return;

        RightForearm.localPosition += (m_forearm_hit_pos - RightForearm.localPosition);
        RightForearm.localRotation = Quaternion.Slerp(RightForearm.localRotation, m_forearm_hit_rot, m_rtime_count);

        BhapticsLibrary.Play("righthit");

        RightBicep.localPosition += (m_bicep_hit_pos - RightBicep.localPosition) * Time.deltaTime * 4;
        RightBicep.localRotation = Quaternion.Slerp(RightBicep.localRotation, m_bicep_hit_rot, m_rtime_count);

        m_rtime_count = m_rtime_count + Time.deltaTime;

        if (RightForearm.localPosition == m_forearm_hit_pos &&
            RightForearm.localRotation == m_forearm_hit_rot &&
            RightBicep.localRotation == m_bicep_hit_rot)
        {
            RightBicep.localPosition = m_bicep_hit_pos;
            Invoke("CanUnPunchingRight", 1f);
        }
    }

    private void RightArmUnPunching()
    {
        if (!m_rcan_unpunching) return;

        RightForearm.localPosition += (m_forearm_idle_pos - RightForearm.localPosition) * Time.deltaTime * 2;
        RightForearm.localRotation = Quaternion.Slerp(RightForearm.localRotation, m_forearm_idle_rot, m_rtime_count / 2);

        RightBicep.localPosition += (m_bicep_idle_pos - RightBicep.localPosition) * Time.deltaTime * 10;
        RightBicep.localRotation = Quaternion.Slerp(RightBicep.localRotation, m_bicep_idle_rot, m_rtime_count);

        m_rtime_count = m_rtime_count + Time.deltaTime;

        if (RightBicep.localRotation == m_bicep_idle_rot &&
            (Vector3.Distance(m_bicep_idle_pos, RightBicep.localPosition) < 0.1f) &&
            (Vector3.Distance(m_forearm_idle_pos, RightForearm.localPosition) < 0.1f) &&
            RightForearm.localRotation == m_forearm_idle_rot)
        {
            m_rtime_count = 0;
            m_rcan_unpunching = false;
            IsRightArmPunching = false;
        }
    }

    private void LeftArmPunching()
    {
        if (m_lcan_unpunching) return;

        LeftForearm.localPosition += (m_forearm_hit_pos - LeftForearm.localPosition);
        LeftForearm.localRotation = Quaternion.Slerp(LeftForearm.localRotation, m_forearm_hit_rot, m_ltime_count);

        BhapticsLibrary.Play("lefthit");

        LeftBicep.localPosition += (m_bicep_hit_pos - LeftBicep.localPosition) * Time.deltaTime * 4;
        LeftBicep.localRotation = Quaternion.Slerp(LeftBicep.localRotation, m_bicep_hit_rot, m_ltime_count);

        m_ltime_count = m_ltime_count + Time.deltaTime;

        if (LeftForearm.localPosition == m_forearm_hit_pos &&
            LeftForearm.localRotation == m_forearm_hit_rot &&
            LeftBicep.localRotation == m_bicep_hit_rot)
        {
            LeftBicep.localPosition = m_bicep_hit_pos;
            Invoke("CanUnPunchingLeft", 1f);
        }
    }

    private void LeftArmUnPunching()
    {
        if (!m_lcan_unpunching) return;

        LeftForearm.localPosition += (m_forearm_idle_pos - LeftForearm.localPosition) * Time.deltaTime * 2;
        LeftForearm.localRotation = Quaternion.Slerp(LeftForearm.localRotation, m_forearm_idle_rot, m_ltime_count / 2);

        LeftBicep.localPosition += (m_bicep_idle_pos - LeftBicep.localPosition) * Time.deltaTime * 10;
        LeftBicep.localRotation = Quaternion.Slerp(LeftBicep.localRotation, m_bicep_idle_rot, m_ltime_count);

        m_ltime_count = m_ltime_count + Time.deltaTime;

        if (LeftBicep.localRotation == m_bicep_idle_rot &&
            (Vector3.Distance(m_bicep_idle_pos, LeftBicep.localPosition) < 0.1f) &&
            (Vector3.Distance(m_forearm_idle_pos, LeftForearm.localPosition) < 0.1f) &&
            LeftForearm.localRotation == m_forearm_idle_rot)
        {
            m_ltime_count = 0;
            m_lcan_unpunching = false;
            IsLeftArmPunching = false;
        }
    }

    private void CanUnPunchingLeft()
    {
        m_lcan_unpunching = true;
        m_ltime_count = 0;
    }

    private void CanUnPunchingRight()
    {
        m_rcan_unpunching = true;
        m_rtime_count = 0;
    }
}