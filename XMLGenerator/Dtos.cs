using System.Xml.Serialization;

[XmlRoot(ElementName = "TCG", Namespace = "http://tempuri.org/TCG.xsd")]
public class TCG
{
    [XmlElement(ElementName = "TCGHeader")]
    public TCGHeader TCGHeader { get; set; }
    [XmlElement(ElementName = "TCGOrder")]
    public List<TCGOrder> TCGOrder { get; set; }
}

public class TCGHeader
{
    [XmlElement(ElementName = "CustomerOrderID")]
    public string CustomerOrderID { get; set; }
}

public class TCGOrder
{
    [XmlElement(ElementName = "CustomerOrderID")]
    public string CustomerOrderID { get; set; }
    [XmlElement(ElementName = "PatientName")]
    public string PatientName { get; set; }
    [XmlElement(ElementName = "PatientAccountNumber")]
    public string PatientAccountNumber { get; set; }
    [XmlElement(ElementName = "PatientDOB")]
    public DateTime PatientDOB { get; set; }
    [XmlElement(ElementName = "Insurance")]
    public string Insurance { get; set; }
    [XmlElement(ElementName = "InsuranceGroup")]
    public string InsuranceGroup { get; set; }
    [XmlElement(ElementName = "Facility")]
    public string Facility { get; set; }
    [XmlElement(ElementName = "RoomNumber")]
    public string RoomNumber { get; set; }
    [XmlElement(ElementName = "DoctorName")]
    public string DoctorName { get; set; }
    [XmlElement(ElementName = "RPhInitials")]
    public string RPhInitials { get; set; }
    [XmlElement(ElementName = "DispenseDateTime")]
    public DateTime DispenseDateTime { get; set; }
    [XmlElement(ElementName = "DrugID")]
    public string DrugID { get; set; }
    [XmlElement(ElementName = "RxNumber")]
    public string RxNumber { get; set; }
    [XmlElement(ElementName = "Quantity")]
    public double Quantity { get; set; }
    [XmlElement(ElementName = "Instructions")]
    public string Instructions { get; set; }
    [XmlElement(ElementName = "DaysSupply")]
    public int DaysSupply { get; set; }
    [XmlElement(ElementName = "RefillMessage")]
    public string RefillMessage { get; set; }
    [XmlElement(ElementName = "Warning")]
    public string Warning { get; set; }
    [XmlElement(ElementName = "OriginalFillDate")]
    public DateTime OriginalFillDate { get; set; }
}