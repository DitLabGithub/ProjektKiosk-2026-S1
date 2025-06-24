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
}
