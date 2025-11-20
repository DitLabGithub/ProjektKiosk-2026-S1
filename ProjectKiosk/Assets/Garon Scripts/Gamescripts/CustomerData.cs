using UnityEngine;

[System.Serializable]
public class CustomerData
{
    public string Name;
    public string DOB;
    public string Address;
    public string Issuer;
    public Sprite ProfileImage;

    public bool allowNameAccess = false;
    public bool allowDOBAccess = false;
    public bool allowAddressAccess = false;
    public bool allowIssuerAccess = false;
    public bool allowPictureAccess = false;

    // Authorization fields (for Owner IDs)
    public bool isAuthorizationID = false;
    public string authorizationStatus = "Pending";

    // Constructor to copy data from CustomerID prefab component
    public CustomerData(CustomerID id)
    {
        Name = id.Name;
        DOB = id.DOB;
        Address = id.Address;
        Issuer = id.Issuer;
        ProfileImage = id.ProfileImage;

        allowNameAccess = id.allowNameAccess;
        allowDOBAccess = id.allowDOBAccess;
        allowAddressAccess = id.allowAddressAccess;
        allowIssuerAccess = id.allowIssuerAccess;
        allowPictureAccess = id.allowPictureAccess;

        // Copy authorization fields
        isAuthorizationID = id.isAuthorizationID;
        authorizationStatus = id.authorizationStatus;
    }
}
