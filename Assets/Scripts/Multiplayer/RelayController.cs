using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class RelayController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI joinLabel;
    [SerializeField]
    private TMP_InputField serverCodeInputField;
    [SerializeField]
    private Button hostButton, joinButton;
    [SerializeField]
    GameObject canvasGroup;

    private void Awake()
    {
        hostButton.onClick.AddListener(CreateRelay);
        joinButton.onClick.AddListener(JoinRelay);
    }

    async void Start()
    {
        // Execute and wait for task InitializeAsync to finish before executing the rest of the Start function
        await UnityServices.InitializeAsync();
        // Subscribe to the SignedIn action to execute OnSigninComplete

        AuthenticationService.Instance.SignedIn += OnSigninComplete;

        // Execute and wait for task SignInAnonymouslyAsync to finish before executing the rest of the Start function
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    void OnSigninComplete()
    {
        Debug.Log($"sign in completed: {AuthenticationService.Instance.PlayerId}");
    }
    
    public async void CreateRelay()
    {
        // Tries to connect to a relay server
        try
        {
            // Request relay server to connect 4 players max
            var allocation = await RelayService.Instance.CreateAllocationAsync(4);
            // Requests join code
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinLabel.SetText(joinCode);

            // Server initialization
            var serverData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
            NetworkManager.Singleton.StartHost();
            canvasGroup.SetActive(false);
        }

        // Catches a specific error related to the relay service
        catch (RelayServiceException e)
        {
            Debug.Log(e);

            // Stop this function if no relay connection is found
            throw;
        }
    }

    public async void JoinRelay()
    {
        try
        {
            // Searches for existing allocation with join code ID
            var allocation = await RelayService.Instance.JoinAllocationAsync(serverCodeInputField.text);
            var serverData = new RelayServerData(allocation, "dtls");
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
            NetworkManager.Singleton.StartClient();
            joinLabel.SetText(serverCodeInputField.text);
            canvasGroup.SetActive(false);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            throw;
        }
        
    }
}
