using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManagement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] bool joinForTest;

    [Header("References")]
    [SerializeField] PlayerInputManager playerInputManager;

    //[Space(10)]
    // RSO
    // RSF
    // RSP

    [Header("Input")]
    [SerializeField] RSE_EnableJoining rseEnableJoining;
    [SerializeField] RSE_DisableJoining rseDisableJoining;

	//[Header("Output")]

	private void OnEnable()
    {
		playerInputManager.onPlayerJoined += OnPlayerJoined;
		rseEnableJoining.action += playerInputManager.EnableJoining;
        rseDisableJoining.action += playerInputManager.DisableJoining;
    }
    private void OnDisable()
    {
		playerInputManager.onPlayerJoined -= OnPlayerJoined;
		rseEnableJoining.action -= playerInputManager.EnableJoining;
        rseDisableJoining.action -= playerInputManager.DisableJoining;
    }

    private void Start()
    {
        if(!joinForTest) playerInputManager.DisableJoining();
    }

	private void OnPlayerJoined(PlayerInput playerInput)
	{
        if(playerInput.TryGetComponent(out BlobMotor blob))
        {
            blob.OnJoined();
        }
	}
}