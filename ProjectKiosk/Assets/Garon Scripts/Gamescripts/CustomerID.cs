using UnityEngine;

public class CustomerID : MonoBehaviour
{
    [Header("ID Info")]
    public Sprite ProfileImage;
    public string Name;
    public string DOB;
    public string Address;
    public string Issuer;

    [Header("Access Control")]
    public bool allowDOBAccess = false;
    public bool allowAddressAccess = false;
    public bool allowPictureAccess = false;
    public bool allowNameAccess = false;
    public bool allowIssuerAccess = false;

    [Header("Authorization (for Owner IDs)")]
    [Tooltip("Is this an authorization ID (like Owner's ID)?")]
    public bool isAuthorizationID = false;

    [Tooltip("Authorization status (e.g., 'Pending', 'Authorized ✓', 'Denied')")]
    public string authorizationStatus = "Pending";
}
