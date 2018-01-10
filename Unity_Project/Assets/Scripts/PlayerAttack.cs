using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {

	public float m_Strength = 1.5f;

	bool m_IsAttack = false;
	float m_AttackTime = 0.1f;

	PlayerController m_ParentPlayer;

	void Awake()
	{
		m_ParentPlayer = GetComponentInParent<PlayerController> ();
	}

	void OnTriggerEnter(Collider other)
	{
		Attack (other);
	}

	void OnTriggerStay(Collider other)
	{
		Attack (other);
	}

	void Attack(Collider other)
	{
		if (!other.CompareTag ("Player"))
			return;

		if (!m_IsAttack)
			return;

		PlayerController otherPlayer = other.GetComponent<PlayerController> ();

		Vector3 attackDirection = m_ParentPlayer.GetCurrentFaceDirection();
		otherPlayer.PushBackFromAttack (attackDirection, m_Strength);
	}

	void Update()
	{
		if (Time.time > m_ParentPlayer.GetLastPunchTime() + m_AttackTime)
			m_IsAttack = false;
	}
		
	public void ActivateAttack()
	{
		m_IsAttack = true;
	}
}
