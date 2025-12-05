using System.Collections;
using UnityEngine;

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
        m_forearm_idle_pos = new Vector3(62.1899986f, 71.6900024f, -62.5f);
        m_forearm_idle_rot = new Quaternion(0.579913378f, -0.31349498f, 0.193250179f, 0.726688325f);
        m_forearm_hit_pos = new Vector3(58.6199989f, 60.4700012f, -59.7599983f);
        m_forearm_hit_rot = new Quaternion(0.428557694f, -0.226369068f, 0.233641773f, 0.842915714f);
        m_bicep_idle_pos = new Vector3(19.4599991f, 13.1038933f, -34.9103317f);
        m_bicep_idle_rot = new Quaternion(0.234761238f, -0.217837095f, 0.054071188f, 0.945785642f);
        m_bicep_hit_pos = new Vector3(19.4599991f, 30.5494537f, -68.1299973f);
        m_bicep_hit_rot = new Quaternion(0.402451813f, -0.22403188f, 0.0136534264f, 0.887499809f);
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
        // Проверяем тег контроллера
        if (other.CompareTag("LeftController"))
        {
            // Проверяем, чтобы этот скрипт был на объекте с тегом "Hit"
            if (gameObject.CompareTag("Hit") && !IsLeftArmPunching)
            {
                IsLeftArmPunching = true;
            }
        }
        else if (other.CompareTag("RightController"))
        {
            // Проверяем, чтобы этот скрипт был на объекте с тегом "Hit"
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
        RightForearm.localRotation = Quaternion.Slerp(RightForearm.localRotation, m_forearm_idle_rot, m_rtime_count);

        RightBicep.localPosition += (m_bicep_idle_pos - RightBicep.localPosition) * Time.deltaTime * 2;
        RightBicep.localRotation = Quaternion.Slerp(RightBicep.localRotation, m_bicep_idle_rot, m_rtime_count);

        m_rtime_count = m_rtime_count + Time.deltaTime;

        if (RightBicep.localRotation == m_bicep_idle_rot &&
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
        LeftForearm.localRotation = Quaternion.Slerp(LeftForearm.localRotation, m_forearm_idle_rot, m_ltime_count);

        LeftBicep.localPosition += (m_bicep_idle_pos - LeftBicep.localPosition) * Time.deltaTime * 2;
        LeftBicep.localRotation = Quaternion.Slerp(LeftBicep.localRotation, m_bicep_idle_rot, m_ltime_count);

        m_ltime_count = m_ltime_count + Time.deltaTime;

        if (LeftBicep.localRotation == m_bicep_idle_rot &&
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