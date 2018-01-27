using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // --------------------------------------------------------------

    // The character's running speed
    [SerializeField]
    float m_RunSpeed = 5.0f;

    // The gravity strength
    [SerializeField]
    float m_Gravity = 60.0f;

    // The maximum speed the character can fall
    [SerializeField]
    float m_MaxFallSpeed = 20.0f;

	// The character's jump height 
    [SerializeField]
    float m_JumpHeight = 4.0f;

    // Identifier for Input
    [SerializeField]
    string m_PlayerInputString = "_P1";

	// Added field: 
	public float m_Drag = 0.5f;

	// Added field:
	public bool m_IsAI = false;

    // --------------------------------------------------------------

    // The charactercontroller of the player
    CharacterController m_CharacterController;

	// Added field:
	GameObject[] m_AllPlayers;

	// Added field:
	Animator m_Animator;

	// Added field:
	PlayerAttack m_PlayerAttack;

    // The current movement direction in x & z.
    Vector3 m_MovementDirection = Vector3.zero;

	// Added field:
	Vector3 m_FaceDirection = Vector3.zero;

    // The current movement speed
    float m_MovementSpeed = 0.0f;

    // The current vertical / falling speed
    float m_VerticalSpeed = 0.0f;

	// Added field:
	Vector3 m_PushedDirection = Vector3.zero;

	// Added field:
	float m_PushedSpeed = 0.0f;

    // The current movement offset
    Vector3 m_CurrentMovementOffset = Vector3.zero;

    // The starting position of the player
    Vector3 m_SpawningPosition = Vector3.zero;

    // Whether the player is alive or not
    bool m_IsAlive = true;

	// Added field: To ensure that the push is only evaluated once (fixed irregular movement bug from jumping)
	bool m_HasCollidedWithPlayer = false;

	// Added field:
	bool m_IsAttacked = false;

	// Added fields: To check whether the AI player is near the map bounds, and on which section
	bool m_AIIsNearOOB = false;
	Collider m_OOBMeshCollider;

	// Added fields:
	const float PUNCH_COOLDOWN = 0.5f;
	float m_LastPunchTime = 0.0f;

    // The time it takes to respawn
    const float MAX_RESPAWN_TIME = 1.0f;
    float m_RespawnTime = MAX_RESPAWN_TIME;

    // --------------------------------------------------------------

    void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
		m_Animator = GetComponent<Animator> ();
		m_PlayerAttack = GetComponentInChildren<PlayerAttack> ();
		m_AllPlayers = GameObject.FindGameObjectsWithTag ("Player");
    }

    // Use this for initialization
    void Start()
    {
        m_SpawningPosition = transform.position;
    }

    void Jump()
    {
        m_VerticalSpeed = Mathf.Sqrt(m_JumpHeight * m_Gravity);
    }

    void ApplyGravity()
    {
        // Apply gravity
        m_VerticalSpeed -= m_Gravity * Time.deltaTime;

        // Make sure we don't fall any faster than m_MaxFallSpeed.
        m_VerticalSpeed = Mathf.Max(m_VerticalSpeed, -m_MaxFallSpeed);
        m_VerticalSpeed = Mathf.Min(m_VerticalSpeed, m_MaxFallSpeed);
    }

	// Added method:
	public void PushBackFromAttack(Vector3 pushDirection, float pushStrength)
	{
		m_IsAttacked = true;
		m_PushedDirection = pushDirection;

		// We treat m_RunSpeed as 1 unit of speed
		m_PushedSpeed = m_RunSpeed * pushStrength;
	}

	// Added method:
	public Vector3 GetCurrentFaceDirection()
	{
		return m_FaceDirection;
	}

	// Added method: 
	void OnControllerColliderHit (ControllerColliderHit other)
	{
		if (!other.gameObject.CompareTag ("Player")) 
		{
			m_HasCollidedWithPlayer = false;
			return;
		}

		PlayerController otherPlayer = other.gameObject.GetComponent<PlayerController> ();

		if (!m_HasCollidedWithPlayer) 
		{
			otherPlayer.m_PushedDirection = m_MovementDirection;
			otherPlayer.m_PushedSpeed = m_MovementSpeed;
			m_HasCollidedWithPlayer = true;
		}
	}

	// Added method:
	void OnControllerColliderStay(ControllerColliderHit other)
	{
		if (other.gameObject.CompareTag ("OutOfBounds") && m_IsAI) 
		{
			m_AIIsNearOOB = true;
			m_OOBMeshCollider = other.collider;
		}
	}

	// Added method:
	void OnControllerColliderExit(ControllerColliderHit other)
	{
		if (other.gameObject.CompareTag ("OutOfBounds") && m_IsAI) 
		{
			m_AIIsNearOOB = false;
			m_OOBMeshCollider = null;
		}
	}

    void UpdateMovementState()
    {
		m_MovementDirection = m_IsAI ? UpdateAIMovement() + m_PushedDirection : UpdatePlayerMovement() + m_PushedDirection;

		if (m_MovementDirection != Vector3.zero)
			m_FaceDirection = m_MovementDirection;

		float actualMovementSpeed = m_RunSpeed + m_PushedSpeed;
		m_MovementSpeed = actualMovementSpeed;

		// Reset the pushed direction and speed
		if (m_IsAttacked) 
		{
			m_PushedSpeed -= m_Drag;

			if (m_PushedSpeed <= 0.0f)
				m_IsAttacked = false;
		} 
		else
			ResetPushVelocity ();
    }

	// Added method:
	Vector3 UpdatePlayerMovement()
	{
		// Get Player's movement input and determine direction and set run speed
		float horizontalInput = Input.GetAxisRaw("Horizontal" + m_PlayerInputString);
		float verticalInput = Input.GetAxisRaw("Vertical" + m_PlayerInputString);

		return new Vector3 (horizontalInput, 0, verticalInput);
	}

	// Added method: 
	Vector3 UpdateAIMovement()
	{
		// Find the closest player
		Vector3[] offsetsFromEachPlayer = new Vector3[m_AllPlayers.Length];
		for (int playerIndex = 0; playerIndex < m_AllPlayers.Length; playerIndex++) 
		{
			PlayerController currentPlayerController = m_AllPlayers[playerIndex].GetComponent<PlayerController> ();
			offsetsFromEachPlayer [playerIndex] = currentPlayerController.transform.position - transform.position;
		}

		int unsortedLength = offsetsFromEachPlayer.Length;
		bool magnitudesSwapped;
		do {
			magnitudesSwapped = false;
			for (int swapIndex = 0; swapIndex < unsortedLength - 1; swapIndex++) 
			{
				if (offsetsFromEachPlayer [swapIndex].magnitude > offsetsFromEachPlayer [swapIndex + 1].magnitude) 
				{
					Vector3 offsetBuffer = offsetsFromEachPlayer [swapIndex];
					offsetsFromEachPlayer [swapIndex] = offsetsFromEachPlayer [swapIndex + 1];
					offsetsFromEachPlayer [swapIndex + 1] = offsetBuffer;
					magnitudesSwapped = true;
				}
			}
			unsortedLength--;
		} while (!magnitudesSwapped);

		// Note: closest distance will be in index 1 after the swap as index 0 would refer to the current player itself
		// Set the horizontal and vertical movement direction based on the distance
		Vector3 movementDirection = Vector3.zero;

		if (offsetsFromEachPlayer [1].x > 0)
			movementDirection.x = 1;
		else if (offsetsFromEachPlayer [1].x < 0)
			movementDirection.x = -1;
		
		if (offsetsFromEachPlayer [1].z > 0)
			movementDirection.z = 1;
		else if (offsetsFromEachPlayer [1].z < 0)
			movementDirection.z = -1;

		// Avoid the OOB quad colliders, move out of them if necessary
		if (m_AIIsNearOOB && m_OOBMeshCollider != null) 
		{
			Vector3 distanceToBoundsCentre = m_OOBMeshCollider.bounds.center - transform.position;

			if (offsetsFromEachPlayer [1].x > 0 && distanceToBoundsCentre.x > 0)
				movementDirection.x = 0;
			else if (offsetsFromEachPlayer [1].x < 0 && distanceToBoundsCentre.x < 0)
				movementDirection.x = 0;

			if (offsetsFromEachPlayer [1].z > 0 && distanceToBoundsCentre.z > 0)
				movementDirection.z = 0;
			else if (offsetsFromEachPlayer [1].z < 0 && distanceToBoundsCentre.z < 0)
				movementDirection.z = 0;
		}

		return movementDirection;
	}

    void UpdateJumpState()
    {
        // Character can jump when standing on the ground
        if (Input.GetButtonDown("Jump" + m_PlayerInputString) && m_CharacterController.isGrounded)
        {
            Jump();
        }
    }

	// Added method:
	void UpdateAttack()
	{
		if (Input.GetButton ("Fire1" + m_PlayerInputString) && Time.time > m_LastPunchTime + PUNCH_COOLDOWN) 
		{
			m_LastPunchTime = Time.time;
			m_Animator.SetTrigger ("Punch");
			m_PlayerAttack.ActivateAttack();
		}
	}

	public float GetLastPunchTime()
	{
		return m_LastPunchTime;
	}

    // Update is called once per frame
    void Update()
    {
        // If the player is dead update the respawn timer and exit update loop
        if(!m_IsAlive)
        {
            UpdateRespawnTime();
            return;
        }

        // Update movement input
        UpdateMovementState();

        // Update jumping input and apply gravity
        UpdateJumpState();
        ApplyGravity();

		// Added step:
		UpdateAttack();

        // Calculate actual motion
        m_CurrentMovementOffset = (m_MovementDirection * m_MovementSpeed + new Vector3(0, m_VerticalSpeed, 0)) * Time.deltaTime;

        // Move character
        m_CharacterController.Move(m_CurrentMovementOffset);

        // Rotate the character in movement direction
        if(m_MovementDirection != Vector3.zero)
        {
            RotateCharacter(m_MovementDirection);
        }
    }

    void RotateCharacter(Vector3 movementDirection)
    {
        Quaternion lookRotation = Quaternion.LookRotation(movementDirection);
        if (transform.rotation != lookRotation)
        {
            transform.rotation = lookRotation;
        }
    }

    public int GetPlayerNum()
    {
        if(m_PlayerInputString == "_P1")
        {
            return 1;
        }
        else if (m_PlayerInputString == "_P2")
        {
            return 2;
        }

        return 0;
    }

    public void Die()
    {
        m_IsAlive = false;
        m_RespawnTime = MAX_RESPAWN_TIME;
    }

    void UpdateRespawnTime()
    {
        m_RespawnTime -= Time.deltaTime;
        if (m_RespawnTime < 0.0f)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        m_IsAlive = true;
		ResetPushVelocity ();
        transform.position = m_SpawningPosition;
        transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
    }

	void ResetPushVelocity()
	{
		m_PushedDirection = Vector3.zero;
		m_PushedSpeed = 0.0f;
	}
}
