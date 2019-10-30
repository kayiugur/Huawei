using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UblDespatchAdvice;
using Ionic.Zlib;
using System.IO;
using Istr.Edespatch;
using Istr.Edespatch.Edespatch.Reference;
using Ionic.Zip;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Xml;
using System.Runtime.InteropServices;
using AppResp = AppResObject;


namespace Istr.Edespatch
{
    [ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    public class FitService : Istr.Edespatch.IFitService
    {
        public string ConnectionUserName { get; set; }
        public string ConnectionPassword { get; set; }
        public string ConnectionMerchantId { get; set; }
        public string ConnectionUrl { get; set; }
        public string Fit_Vkn { get; set; }
        public string Fit_Alias_As_Sender { get; set; }
        public getDesUserListRequest Request { get; set; }
        public getDesUBLListRequest Receive_Request { get; set; }
        public Invoice Invoice { get; set; }
        public UserList UserTypeList { get; set; }
        public UserList[] UserType { get; set; }
        private getDesUserListResponse responce;
        private getDesUBLListResponse receive_response;
        public GetDesUBLListResponseType[] receive_response2;
        public getDesUBLResponse SysResponce;
        public ReceiptAdvice SystemResponceXML;
        public bool DebugMode { set; get; }
        public string Error { set; get; }
        public string ReturnId { set; get; }
        public string ReturnUuid { set; get; }
        public string ReturnEnvUuid { set; get; }
        public string RequestXml { set; get; }
        public string ResponceXml { set; get; }
        public bool HideAdditDocRefference { set; get; }
        public string SuplTicaretSicilNo { set; get; }
        public string SuplMersisNo { set; get; }
        public string PdfFileName { set; get; }
        private getDesUBLListResponse responceApp;
        public getDesUBLListRequest RequestApp { get; set; }
        public string RequestandResponceLocation { set; get; }
        public bool ShowContractAdditionalDocRef { get; set; }
        public bool ShowCatalogTypeAdditDocRef { get; set; }


        #region Main
        private string GetAuthorization()
        {
            string authorization = ConnectionUserName + ":" + ConnectionPassword; //kullanıcı adı ve şifre. aralarında : karakteri olacak.
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(authorization);
            string base64authorization = Convert.ToBase64String(byteArray);

            return string.Format("Basic {0}", base64authorization);
        }

        private ClientEDespatchServicesPortClient CreateWSClient()
        {
            var binding = new BasicHttpsBinding();
            binding.Name = "FitIntegration";
            binding.SendTimeout = TimeSpan.FromMinutes(1);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(5);
            binding.MaxBufferPoolSize = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;

            var endpoint = new EndpointAddress(ConnectionUrl);
            return new ClientEDespatchServicesPortClient(binding, endpoint);
        }
        #endregion Main

        #region GetCompanies
        public bool GetUserList(getDesUserListRequest req)
        {
            DateTime ttime = DateTime.Now;
            ClientEDespatchServicesPortClient wsClient = CreateWSClient();
            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)wsClient.InnerChannel))
            {
                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add(HttpRequestHeader.Authorization, GetAuthorization());
                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer4 =
                       new System.Xml.Serialization.XmlSerializer(typeof(getDesUserListRequest));
                    System.IO.StreamWriter file4 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "GetDesUserListRequest.txt");
                    writer4.Serialize(file4, req);
                    file4.Close();
                }

                responce = wsClient.getDesUserList(req);

                var xml = IonicUNZipFile(responce.DocData);
                MemoryStream tempS = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                StreamReader reader = new StreamReader(tempS);
                XmlSerializer SerializerObj = new XmlSerializer(typeof(UserList));
                UserTypeList = (UserList)SerializerObj.Deserialize(reader);
                reader.Close();


                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer5 =
                       new System.Xml.Serialization.XmlSerializer(typeof(UserList));
                    System.IO.StreamWriter file5 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "GetDesUserListResponce.txt");
                    writer5.Serialize(file5, UserTypeList);
                    file5.Close();
                }
            }
            return true;
        }

        public Int32 GetUserCount()
        {
            if (UserTypeList.User != null)
            {
                return UserTypeList.User.Count();
            }
            else
            {
                return 0;
            }
        }
        public User GetUser(Int32 key)
        {
            if (UserTypeList.User != null)
            {
                return UserTypeList.User[key];
            }
            else
            {
                return new User();
            }
        }
        #endregion GetCompanies

        #region Send Despatch

        public string GetDespatchXml()
        {
            if (DebugMode)
            {
                System.Xml.Serialization.XmlSerializer writer3 =
                   new System.Xml.Serialization.XmlSerializer(Invoice.GetType());
                System.IO.StreamWriter file3 = new System.IO.StreamWriter(
                    RequestandResponceLocation + "GetDespatchXML_invoice.txt");
                writer3.Serialize(file3, Invoice);
                file3.Close();
            }
            DespatchAdviceType createdUbl = CreateUBL();
            return GetXML(createdUbl);

        }

        public string GetDespatchXmlForExportDespatch()
        {
            if (DebugMode)
            {
                System.Xml.Serialization.XmlSerializer writer3 =
                   new System.Xml.Serialization.XmlSerializer(Invoice.GetType());
                System.IO.StreamWriter file3 = new System.IO.StreamWriter(
                    RequestandResponceLocation + "GetDespatchForExportDespatchXML_invoice.txt");
                writer3.Serialize(file3, Invoice);
                file3.Close();
            }

            DespatchAdviceType createdUblExportInv = CreateUBL_ForExportDespatch();
            return GetXML(createdUblExportInv);

        }

        public Boolean SendDespatchForExportDespatch()
        {
            //to include in the project into the UBL-Invoice-2_1.cs files we include in our project by adding namespace.
            DespatchAdviceType createdUbl = CreateUBL_ForExportDespatch(); //We set the parameters of the bill we send to this method.

            if (DebugMode)
            {
                System.Xml.Serialization.XmlSerializer writer3 =
                    new System.Xml.Serialization.XmlSerializer(typeof(DespatchAdviceType));
                System.IO.StreamWriter file3 = new System.IO.StreamWriter(
                    RequestandResponceLocation + "createdUbl.xml");
                writer3.Serialize(file3, createdUbl);
                file3.Close();
            }

            RequestXml = GetXML(createdUbl); //CreateUBL (the top part of the project), we turn to the XML data returned from the method. In the first method, you can copy ready.

            if (DebugMode)
            {
                File.WriteAllText(RequestandResponceLocation + "FITValidateXMLInputString.xml", RequestXml);
            }


            //ValidateXML(strFatura);

            byte[] byteFatura = System.Text.Encoding.ASCII.GetBytes(RequestXml); //xml We translate the byte data type.

            //here should be considered part Zip File () method (in the upper part of the project) is çalışmat only in .net 4.5. 3rd party zip is used for pre-ionic system. DLL available in the project.
            byte[] zipliFile = IonicZipFile(RequestXml, createdUbl.UUID.Value); //The bill is converted into XML are adding zip file.

            //Another point, the incoming XML data and the zip file do not record any physical file location. We are sending the data stored in memory.
            ClientEDespatchServicesPortClient wsClient = CreateWSClient();

            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)wsClient.InnerChannel))
            {
                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add(HttpRequestHeader.Authorization, GetAuthorization());

                var req = new sendDesUBLRequest()
                {
                    SenderIdentifier = Fit_Alias_As_Sender, //sender unit label
                    ReceiverIdentifier = Invoice.RecepientAlias, //recipient mailbox
                    VKN_TCKN = Fit_Vkn, //TC or tax number
                    DocType = "DESPATCH", //transmitted document type. envelopes, invoices, etc.
                    DocData = zipliFile //xml file in the zipped file. //dikkat...                        
                };

                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer2 =
                        new System.Xml.Serialization.XmlSerializer(typeof(sendDesUBLRequest));
                    System.IO.StreamWriter file2 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "FITSendDespatchRequest.xml");
                    writer2.Serialize(file2, req);
                    file2.Close();
                }

                sendDesUBLResponse result = wsClient.sendDesUBL(req);

                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer1 =
                        new System.Xml.Serialization.XmlSerializer(typeof(SendDesUBLResponseType[]));
                    System.IO.StreamWriter file1 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "FITSendDespatchResponce.xml");
                    writer1.Serialize(file1, result);
                    file1.Close();
                }
                var stringwriter = new System.IO.StringWriter();
                var serializer = new XmlSerializer(typeof(SendDesUBLResponseType[]));
                serializer.Serialize(stringwriter, result);
                ResponceXml = stringwriter.ToString();

                ReturnId = result.Response[0].ID;
                ReturnUuid = result.Response[0].UUID;
                ReturnEnvUuid = result.Response[0].EnvUUID;

                return true;
            }
        }

        public void SaveDespatchXml(string path)
        {
            DespatchAdviceType createdUbl = CreateUBL();
            File.WriteAllText(path, GetXML(createdUbl));
        }

        public void SaveDespatchXmlForExportDespatch(string path)
        {
            DespatchAdviceType createdUbl = CreateUBL_ForExportDespatch();
            File.WriteAllText(path, GetXML(createdUbl));
        }

        public Boolean SendDespatch()
        {
            //to include in the project into the UBL-Invoice-2_1.cs files we include in our project by adding namespace.
            DespatchAdviceType createdUbl = CreateUBL(); //We set the parameters of the bill we send to this method.

            if (DebugMode)
            {
                System.Xml.Serialization.XmlSerializer writer3 =
                    new System.Xml.Serialization.XmlSerializer(typeof(DespatchAdviceType));
                System.IO.StreamWriter file3 = new System.IO.StreamWriter(
                    RequestandResponceLocation + "createdUbl.xml"); writer3.Serialize(file3, createdUbl);
                file3.Close();
            }

            RequestXml = GetXML(createdUbl); //CreateUBL (the top part of the project), we turn to the XML data returned from the method. In the first method, you can copy ready.

            if (DebugMode)
            {
                File.WriteAllText(RequestandResponceLocation + "FITValidateXMLInputString.xml", RequestXml);
            }


            byte[] byteFatura = System.Text.Encoding.ASCII.GetBytes(RequestXml); //xml We translate the byte data type.

            //here should be considered part Zip File () method (in the upper part of the project) is çalışmat only in .net 4.5. 3rd party zip is used for pre-ionic system. DLL available in the project.
            byte[] zipliFile = IonicZipFile(RequestXml, createdUbl.UUID.Value); //The bill is converted into XML are adding zip file.

            //Another point, the incoming XML data and the zip file do not record any physical file location. We are sending the data stored in memory.
            ClientEDespatchServicesPortClient wsClient = CreateWSClient();

            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)wsClient.InnerChannel))
            {
                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add(HttpRequestHeader.Authorization, GetAuthorization());
                var req = new sendDesUBLRequest()
                {
                    VKN_TCKN = Fit_Vkn, //TC or tax number
                    SenderIdentifier = Fit_Alias_As_Sender, //sender unit label
                    ReceiverIdentifier = Invoice.RecepientAlias, //recipient mailbox                        
                    DocType = "DESPATCH", //transmitted document type. envelopes, invoices, etc.
                    DocData = zipliFile //xml file in the zipped file.
                };

                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer2 =
                        new System.Xml.Serialization.XmlSerializer(typeof(sendDesUBLRequest));
                    System.IO.StreamWriter file2 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "FITSendDespatchRequest.xml");
                    writer2.Serialize(file2, req);
                    file2.Close();
                }

                sendDesUBLResponse result = wsClient.sendDesUBL(req);

                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer1 =
                        new System.Xml.Serialization.XmlSerializer(typeof(sendDesUBLResponse));
                    System.IO.StreamWriter file1 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "FITSendDespatchResponce.xml");
                    writer1.Serialize(file1, result);
                    file1.Close();
                }
                var stringwriter = new System.IO.StringWriter();
                var serializer = new XmlSerializer(typeof(sendDesUBLResponse));
                serializer.Serialize(stringwriter, result);

                ResponceXml = stringwriter.ToString();
                ReturnId = result.Response[0].ID;
                ReturnUuid = result.Response[0].UUID;
                ReturnEnvUuid = result.Response[0].EnvUUID;


                return true;
            }
        }

        private DespatchLineType[] GetInvoiceLines()
        {
            List<DespatchLineType> DespatchLine = new List<DespatchLineType>();

            int LineNo = 0;
            foreach (var Line in Invoice.DespatchLines)
            {
                LineNo++;

                DespatchLine.Add(
                            new DespatchLineType()
                            {
                                ID = new IDType { Value = LineNo.ToString() },
                                DeliveredQuantity = new DeliveredQuantityType { unitCode = Line.ItemUom, Value = Line.Quantity },

                                OutstandingQuantity = new OutstandingQuantityType { unitCode = Line.ItemUom, Value = Line.OutStanding },

                                OrderLineReference = new OrderLineReferenceType
                                {
                                    LineID = new LineIDType { Value = LineNo.ToString() }
                                },

                                Item = new ItemType
                                {
                                    Name = new NameType1 { Value = Line.ItemName },
                                    SellersItemIdentification = new ItemIdentificationType { ID = new IDType() { Value = Line.ItemNumber } }
                                },
                                Shipment = new ShipmentType[]
                                {
                                    new ShipmentType()
                                    {
                                    ID = new IDType { Value = "" },
                                    GoodsItem = new GoodsItemType[]
                                        {
                                            new GoodsItemType()
                                            {
                                                InvoiceLine = new InvoiceLineType[]
                                                {
                                                    new InvoiceLineType()
                                                    {
                                                        ID = new IDType{ Value = Invoice.DespatchLineId },
                                                        InvoicedQuantity = new InvoicedQuantityType{Value = Line.DespatchQuantity },
                                                        LineExtensionAmount = new LineExtensionAmountType{currencyID = Invoice.DespatchCurrencyCode, Value = Line.LineExtensionAmount},

                                                        Item = new ItemType
                                                        {
                                                            Name = new NameType1 {Value = Invoice.DespLineItemName }
                                                        },

                                                        Price = new PriceType
                                                        {
                                                            PriceAmount = new PriceAmountType
                                                                {
                                                                currencyID = Invoice.DespatchCurrencyCode,
                                                                Value = Line.Price
                                                                }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }

                            }

                    );

            }

            return DespatchLine.ToArray();
        }

        private DespatchLineType[] GetDespatchLinesExportDespatch()
        {
            List<DespatchLineType> DespatchLine = new List<DespatchLineType>();

            int LineNo = 0;
            foreach (var Line in Invoice.DespatchLines)
            {
                LineNo++;

                DespatchLine.Add(

                            new DespatchLineType()
                            {
                                ID = new IDType { Value = LineNo.ToString() },
                                DeliveredQuantity = new DeliveredQuantityType { unitCode = Line.ItemUom, Value = Line.Quantity },

                                OutstandingQuantity = new OutstandingQuantityType { unitCode = Line.ItemUom, Value = Line.OutStanding },

                                OrderLineReference = new OrderLineReferenceType
                                {
                                    LineID = new LineIDType { Value = LineNo.ToString() }
                                },

                                Item = new ItemType
                                {
                                    SellersItemIdentification = new ItemIdentificationType { ID = new IDType() { Value = Line.ItemNumber } },
                                    Name = new NameType1 { Value = Line.ItemName }
                                },
                                Shipment = new ShipmentType[]
                                {
                                    new ShipmentType()
                                    {
                                    ID = new IDType { Value = LineNo.ToString() },
                                    GoodsItem = new GoodsItemType[]
                                        {
                                            new GoodsItemType()
                                            {
                                                InvoiceLine = new InvoiceLineType[]
                                                {
                                                    new InvoiceLineType()
                                                    {
                                                        ID = new IDType{ Value = Invoice.DespatchLineId },
                                                        InvoicedQuantity = new InvoicedQuantityType{Value = Line.Quantity },
                                                        LineExtensionAmount = new LineExtensionAmountType{currencyID = Invoice.DespatchCurrencyCode, Value = Line.LineExtensionAmount},

                                                        Item = new ItemType
                                                        {
                                                            Name = new NameType1 {Value = Invoice.DespLineItemName }
                                                        },

                                                        Price = new PriceType
                                                        {
                                                            PriceAmount = new PriceAmountType
                                                                {
                                                                currencyID = Invoice.DespatchCurrencyCode,
                                                                Value = Line.Price
                                                                }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }

                            }

                    );

            }

            return DespatchLine.ToArray();
        }

        private DespatchAdviceType CreateUBL()
        {
            List<NoteType> despatchNotesList = new List<NoteType>();
            foreach (string DespatchNote in Invoice.Notes)
            {
                if (!String.IsNullOrEmpty(DespatchNote))
                {
                    NoteType noteType = new NoteType() { Value = DespatchNote };
                    despatchNotesList.Add(noteType);
                }
            }

            NoteType[] despatchNotes = despatchNotesList.ToArray();
            List<DocumentReferenceType> DespatchDocumentReferenceList;
            if (Invoice.Shipments.Count() > 0)
            {
                DespatchDocumentReferenceList = new List<DocumentReferenceType>();

                foreach (Shipment currShip in Invoice.Shipments)
                {
                    DocumentReferenceType currDRT = new DocumentReferenceType()
                    {
                        ID = new IDType()
                        {
                            Value = currShip.SalesShipmentNo
                        },
                        IssueDate = new IssueDateType()
                        {
                            Value = currShip.SalesShipmentDate
                        }
                    };
                    DespatchDocumentReferenceList.Add(currDRT);
                }
            }
            else
            {
                DespatchDocumentReferenceList = null;
            }

            OrderReferenceType[] DespatchOrder = null;
            List<OrderReferenceType> OrderReferenceList = new List<OrderReferenceType>();
            foreach (Order curOrder in Invoice.Orders)
            {
                OrderReferenceType currOrderType = new OrderReferenceType()
                {
                    ID = new IDType()
                    {
                        Value = curOrder.OrderNo
                    },
                    IssueDate = new IssueDateType()
                    {
                        Value = curOrder.OrderDate
                    }
                };
                OrderReferenceList.Add(currOrderType);
            }
            DespatchOrder = OrderReferenceList.ToArray();


            ExchangeRateType localPricingExchangeRate = new ExchangeRateType();
            if (string.IsNullOrEmpty(Invoice.TargetCurrencyCode))
            {
                localPricingExchangeRate = null;
            }
            else
            {
                localPricingExchangeRate = new ExchangeRateType()
                {
                    SourceCurrencyCode = new SourceCurrencyCodeType
                    {
                        Value = Invoice.SourceCurrencyCode
                    },
                    TargetCurrencyCode = new TargetCurrencyCodeType
                    {
                        Value = Invoice.TargetCurrencyCode
                    },
                    CalculationRate = new CalculationRateType
                    {
                        Value = Invoice.CalculationRate
                    }
                };

            }
            IDType IDTag;
            if (string.IsNullOrEmpty(Invoice.IdTag))
            {
                IDTag = new IDType { Value = "" };
            }
            else
            {
                IDTag = new IDType { Value = Invoice.IdTag };
            }

            IDType AdditionalDocumentReferenceID;
            if (string.IsNullOrEmpty(Invoice.DespatchNumber))
            {
                AdditionalDocumentReferenceID = null;
            }
            else
            {
                AdditionalDocumentReferenceID = new IDType { Value = Invoice.DespatchNumber };
            }

            AttachmentType InvAttachment = null;
            if (string.IsNullOrEmpty(PdfFileName))
            {
                InvAttachment = null;
            }
            else
            {
                InvAttachment = new AttachmentType
                {
                    EmbeddedDocumentBinaryObject = new EmbeddedDocumentBinaryObjectType()
                    {
                        characterSetCode = "UTF-8",
                        filename = PdfFileName,
                        encodingCode = "Base64",
                        mimeCode = "application/pdf"
                    }
                };
            }

            DocumentReferenceType CatalogDocRefType = null;
            DocumentReferenceType ContractDocReftype = null;

            if (ShowContractAdditionalDocRef)
            {
                IDType AdditDocReferenceIDContract;
                if (string.IsNullOrEmpty(Invoice.ContractType))
                {
                    AdditDocReferenceIDContract = null;
                }
                else
                {
                    AdditDocReferenceIDContract = new IDType { Value = Invoice.ContractType };
                }


                ContractDocReftype = new DocumentReferenceType()
                {
                    ID = AdditDocReferenceIDContract,
                    IssueDate = new IssueDateType
                    {
                        Value = Invoice.ContractTypeDate
                    },
                    DocumentTypeCode = new DocumentTypeCodeType { Value = "Kontrat" }
                };
            }
            if (ShowCatalogTypeAdditDocRef)
            {
                IDType AdditionalDocumentReferencePaymenTypeID;
                if (string.IsNullOrEmpty(Invoice.CatalogType))
                {
                    AdditionalDocumentReferencePaymenTypeID = null;
                }
                else
                {
                    AdditionalDocumentReferencePaymenTypeID = new IDType { Value = Invoice.CatalogType };
                }

                CatalogDocRefType = new DocumentReferenceType()
                {
                    ID = AdditionalDocumentReferencePaymenTypeID,
                    IssueDate = new IssueDateType
                    {
                        Value = Invoice.CatalogTypeDate
                    },
                    DocumentTypeCode = new DocumentTypeCodeType { Value = "Katalog" }
                };
            }

            DocumentReferenceType[] AddDocRef;
            if (HideAdditDocRefference)
            {
                AddDocRef = null;
            }
            else
            {
                AddDocRef = new DocumentReferenceType[]
                {
                    new DocumentReferenceType()
                    {
                        ID = AdditionalDocumentReferenceID,

                        IssueDate = new IssueDateType
                        {
                            Value = Invoice.DespatchDateCust
                        },
                        DocumentTypeCode = new DocumentTypeCodeType { Value = "CUST_DES_ID" },
                        DocumentType = new DocumentTypeType {Value = "CUST_DES_ID" },
                        Attachment = InvAttachment
                    },
                    CatalogDocRefType,
                    ContractDocReftype
                };
            }

            List<PartyIdentificationType> SuplPartyIdentificationType = new List<PartyIdentificationType>();
            SuplPartyIdentificationType.Add(new PartyIdentificationType() { ID = new IDType { schemeID = Invoice.SupplierVknType, Value = Fit_Vkn } }); //Sadace VKN No gönderilecek.
            if (!string.IsNullOrEmpty(SuplTicaretSicilNo))
            {
                SuplPartyIdentificationType.Add(new PartyIdentificationType() { ID = new IDType { schemeID = "TICARETSICILNO", Value = SuplTicaretSicilNo } });
            }
            if (!string.IsNullOrEmpty(SuplMersisNo))
            {
                SuplPartyIdentificationType.Add(new PartyIdentificationType() { ID = new IDType { schemeID = "MERSISNO", Value = SuplMersisNo } });
            }

            Type aa = Type.GetType("System.Xml.XmlElement");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<xml />");

            DespatchAdviceType eirsaliye = new DespatchAdviceType()
            {
                UBLVersionID = new UBLVersionIDType { Value = "2.1" }, //international standard of 2.1 bill
                CustomizationID = new CustomizationIDType { Value = "TR1.2.1" }, //but by using the IOP UBL naming Turkey as TR-specific invoice format 1.2.
                ProfileID = new ProfileIDType { Value = Invoice.DespatchType }, //There are two types of commercial and basic. response system in the commercial invoice (application response) returns.
                CopyIndicator = new CopyIndicatorType { Value = false }, //m copy is ascertained whether tenor
                UUID = new UUIDType { Value = Guid.NewGuid().ToString() }, //invoice ID
                ID = IDTag,
                IssueDate = new IssueDateType
                {
                    Value = Invoice.DespatchDate
                },
                IssueTime = new IssueTimeType
                {
                    Value = Convert.ToDateTime(Invoice.PostingTime)
                },
                DespatchAdviceTypeCode = new DespatchAdviceTypeCodeType { Value = Invoice.DespatchTypeCode },
                Note = despatchNotes,
                LineCountNumeric = new LineCountNumericType { Value = Invoice.DespatchLines.Count() },

                OrderReference = DespatchOrder,
                //AdditionalDocumentReference = DespatchDocumentReferenceList.ToArray(),
                AdditionalDocumentReference = AddDocRef,

                DespatchSupplierParty = new SupplierPartyType
                {
                    Party = new PartyType()
                    {
                        WebsiteURI = new WebsiteURIType { Value = Invoice.SupplierWebSite },

                        PartyIdentification = SuplPartyIdentificationType.ToArray(),

                        PartyName = new PartyNameType { Name = new NameType1 { Value = Invoice.SupplierName } },

                        PostalAddress = new AddressType
                        {
                            ID = new IDType { Value = Invoice.SupplierRoom },
                            StreetName = new StreetNameType { Value = Invoice.SupplierAddress },
                            BuildingNumber = new BuildingNumberType { Value = Invoice.SupplierBuildingNumber },
                            CitySubdivisionName = new CitySubdivisionNameType { Value = Invoice.SupplierCitySubdivisionName },
                            CityName = new CityNameType { Value = Invoice.SupplierCity },
                            PostalZone = new PostalZoneType { Value = Invoice.SupplierPostalCode },
                            Country = new CountryType { Name = new NameType1 { Value = Invoice.SupplierCountry } }
                        },

                        PhysicalLocation = new LocationType1
                        {
                            ID = new IDType { Value = Invoice.SuppPhyTaxId },
                            Address = new AddressType
                            {
                                ID = new IDType { Value = Invoice.SuppPhyId },
                                StreetName = new StreetNameType { Value = Invoice.SuppPhyStreetName },
                                BuildingNumber = new BuildingNumberType { Value = Invoice.SuppPhyBuildingNumber },
                                CitySubdivisionName = new CitySubdivisionNameType { Value = Invoice.SuppPhyCitySubDiv },
                                CityName = new CityNameType { Value = Invoice.SuppPhyCityName },
                                PostalZone = new PostalZoneType { Value = Invoice.SuppPhyPostalZone },
                                Country = new CountryType
                                {
                                    Name = new NameType1 { Value = Invoice.SuppPhyCountry }
                                }
                            }

                        },

                        PartyTaxScheme = new PartyTaxSchemeType
                        {
                            TaxScheme = new TaxSchemeType
                            {
                                Name = new NameType1 { Value = Invoice.SupplierTaxDepartment }
                            }
                        },


                        Contact = new ContactType
                        {
                            Telephone = new TelephoneType { Value = Invoice.SupplierPhone },
                            Telefax = new TelefaxType { Value = Invoice.SupplierFax },
                            ElectronicMail = new ElectronicMailType { Value = Invoice.SupplierEmail }
                        },
                    }


                },

                DeliveryCustomerParty = new CustomerPartyType
                {
                    Party = new PartyType
                    {
                        WebsiteURI = new WebsiteURIType { Value = Invoice.DeliveryCustomerWebSite },

                        PartyIdentification = new PartyIdentificationType[]
                        {
                            new PartyIdentificationType()
                            {
                                ID = new IDType { schemeID = Invoice.DeliveryCustomerVknType, Value = Invoice.DeliveryCustomerVkn }
                            },
                            new PartyIdentificationType
                            {
                                ID = new IDType { schemeID ="MUSTERINO", Value=Invoice.DeliveryCustomerNo }
                            }
                        },

                        PartyName = new PartyNameType
                        {
                            Name = new NameType1 { Value = Invoice.DeliveryCustomerName }
                        },

                        PostalAddress = new AddressType
                        {
                            ID = new IDType { Value = Invoice.DeliveryCustomerId },
                            StreetName = new StreetNameType { Value = Invoice.DeliveryCustomerAddress },
                            BuildingNumber = new BuildingNumberType { Value = Invoice.DeliveryCustomerBuilNumber },
                            CitySubdivisionName = new CitySubdivisionNameType { Value = Invoice.DeliveryCustomerCitySub },
                            CityName = new CityNameType { Value = Invoice.DeliveryCustomerCity },
                            PostalZone = new PostalZoneType { Value = Invoice.DeliveryCustomerPostalCode },
                            Country = new CountryType { Name = new NameType1 { Value = Invoice.DeliveryCustomerCountry } }
                        },

                        PartyTaxScheme = new PartyTaxSchemeType
                        {
                            TaxScheme = new TaxSchemeType { Name = new NameType1 { Value = Invoice.DeliveryCustomerTaxDepartment } }
                        },

                        Contact = new ContactType
                        {
                            Telephone = new TelephoneType { Value = Invoice.DeliveryCustomerPhone },
                            Telefax = new TelefaxType { Value = Invoice.DeliveryCustomerFax },
                            ElectronicMail = new ElectronicMailType { Value = Invoice.DeliveryCustomerEmail }
                        },

                    }

                },

                BuyerCustomerParty = new CustomerPartyType
                {
                    Party = new PartyType
                    {
                        WebsiteURI = new WebsiteURIType { Value = Invoice.BuyerCustomerWebSite },

                        PartyIdentification = new PartyIdentificationType[]
                        {
                            new PartyIdentificationType()
                            {
                                ID = new IDType { schemeID = Invoice.BuyerCustomerVknType, Value = Invoice.BuyerCustomerVkn }
                            },
                            new PartyIdentificationType
                            {
                                ID = new IDType { schemeID = "MUSTERINO", Value=Invoice.BuyerCustomerNo }
                            }
                        },

                        PartyName = new PartyNameType
                        {
                            Name = new NameType1 { Value = Invoice.BuyerCustomerName }
                        },

                        PostalAddress = new AddressType
                        {
                            ID = new IDType { Value = Invoice.BuyerCustomerId },
                            StreetName = new StreetNameType { Value = Invoice.BuyerCustomerAddress },
                            BuildingNumber = new BuildingNumberType { Value = Invoice.BuyerCustomerBuilNumber },
                            CitySubdivisionName = new CitySubdivisionNameType { Value = Invoice.BuyerCustomerCitySub },
                            CityName = new CityNameType { Value = Invoice.BuyerCustomerCity },
                            PostalZone = new PostalZoneType { Value = Invoice.BuyerCustomerPostalCode },
                            Country = new CountryType { Name = new NameType1 { Value = Invoice.BuyerCustomerCountry } }
                        },

                        PartyTaxScheme = new PartyTaxSchemeType
                        {
                            TaxScheme = new TaxSchemeType { Name = new NameType1 { Value = Invoice.BuyerCustomerTaxDepartment } }
                        },

                        Contact = new ContactType
                        {
                            Telephone = new TelephoneType { Value = Invoice.BuyerCustomerPhone },
                            Telefax = new TelefaxType { Value = Invoice.BuyerCustomerFax },
                            ElectronicMail = new ElectronicMailType { Value = Invoice.BuyerCustomerEmail }
                        }

                    }

                },
                //SellerSupplierParty = new SupplierPartyType
                //{
                //    Party = new PartyType
                //    {
                //        WebsiteURI = new WebsiteURIType { Value = invoice.CustomerWebSite },

                //        PartyIdentification = new PartyIdentificationType[]
                //        {
                //            new PartyIdentificationType()
                //            {
                //                ID = new IDType { schemeID = invoice.SellerCustomerVKNType, Value = invoice.SellerCustomerVKN }
                //            },
                //            new PartyIdentificationType
                //            {
                //                ID = new IDType {schemeID = "MUSTERINO", Value=invoice.SellerCustomerNo }
                //            }
                //        },

                //        PartyName = new PartyNameType
                //        {
                //            Name = new NameType1 { Value = invoice.SellerCustomerName }
                //        },

                //        PostalAddress = new AddressType
                //        {
                //            ID = new IDType { Value = invoice.SellerCustomerID },
                //            StreetName = new StreetNameType { Value = invoice.SellerCustomerAddress },
                //            BuildingNumber = new BuildingNumberType { Value = invoice.SellerCustomerBuilNumber },
                //            CitySubdivisionName = new CitySubdivisionNameType { Value = invoice.SellerCustomerCitySub },
                //            CityName = new CityNameType { Value = invoice.SellerCustomerCity },
                //            PostalZone = new PostalZoneType { Value = invoice.SellerCustomerPostalCode },
                //            Country = new CountryType { Name = new NameType1 { Value = invoice.SellerCustomerCountry } }
                //        },

                //        PartyTaxScheme = new PartyTaxSchemeType
                //        {
                //            TaxScheme = new TaxSchemeType { Name = new NameType1 { Value = invoice.SellerCustomerTaxDepartment } }
                //        },

                //        Contact = new ContactType
                //        {
                //            Telephone = new TelephoneType { Value = invoice.SellerCustomerPhone },
                //            Telefax = new TelefaxType { Value = invoice.SellerCustomerFax },
                //            ElectronicMail = new ElectronicMailType { Value = invoice.SellerCustomerEmail }
                //        }

                //    }
                //},
                //OriginatorCustomerParty = new CustomerPartyType
                //{
                //    Party = new PartyType
                //    {
                //        WebsiteURI = new WebsiteURIType { Value = invoice.OrigCustomerWebSite },

                //        PartyIdentification = new PartyIdentificationType[]
                //        {
                //            new PartyIdentificationType()
                //            {
                //                ID = new IDType { schemeID = invoice.OrigCustomerVKNType, Value = invoice.OrigCustomerVKN }
                //            },
                //            new PartyIdentificationType
                //            {
                //                ID = new IDType {schemeID="MUSTERINO", Value = invoice.OrigCustomerNo }
                //            }
                //        },

                //        PartyName = new PartyNameType
                //        {
                //            Name = new NameType1 { Value = invoice.OrigCustomerName }
                //        },

                //        PostalAddress = new AddressType
                //        {
                //            ID = new IDType { Value = invoice.OrigCustomerID },
                //            StreetName = new StreetNameType { Value = invoice.OrigCustomerAddress },
                //            BuildingName = new BuildingNameType { Value = invoice.OrigCustomerBuilNumber },
                //            CitySubdivisionName = new CitySubdivisionNameType { Value = invoice.OrigCustomerCitySub },
                //            CityName = new CityNameType { Value = invoice.OrigCustomerCity },
                //            PostalZone = new PostalZoneType { Value = invoice.OrigCustomerPostalCode },
                //            Country = new CountryType { Name = new NameType1 { Value = invoice.OrigCustomerCountry } }
                //        },

                //        PartyTaxScheme = new PartyTaxSchemeType
                //        {
                //            TaxScheme = new TaxSchemeType { Name = new NameType1 { Value = invoice.OrigCustomerTaxDepartment } }
                //        },

                //        Contact = new ContactType
                //        {
                //            Telephone = new TelephoneType { Value = invoice.OrigCustomerPhone },
                //            Telefax = new TelefaxType { Value = invoice.OrigCustomerFax },
                //            ElectronicMail = new ElectronicMailType { Value = invoice.OrigCustomerEmail }
                //        }

                //    }
                //},
                Shipment = new ShipmentType
                {
                    ID = new IDType { Value = Invoice.ShipmentId },
                    GoodsItem = new GoodsItemType[]
                        {
                           new GoodsItemType
                            {
                            ValueAmount = new ValueAmountType {currencyID = Invoice.DespatchCurrencyCode ,Value=Invoice.ShipmentValueAmount}
                            }
                        },
                    ShipmentStage = new ShipmentStageType[]
                        {
                            new ShipmentStageType
                                {
                                TransportMeans = new TransportMeansType
                                {
                                    RoadTransport = new RoadTransportType {LicensePlateID = new LicensePlateIDType {schemeID ="PLAKA" ,Value= Invoice.ShipmentLicensePlateId } }
                                },
                                DriverPerson = new PersonType[]
                                {
                                    new PersonType
                                    {
                                        FirstName = new FirstNameType { Value = Invoice.ShipmentFirstName },
                                        FamilyName = new FamilyNameType { Value = Invoice.ShipmentFamilyName},
                                        Title = new TitleType { Value = Invoice.ShipmentTitle },
                                        NationalityID = new NationalityIDType { Value = Invoice.ShipmentNationalityId }
                                    }
                                }
                            }
                        },
                    Delivery = new DeliveryType
                    {
                        CarrierParty = new PartyType
                        {
                            PartyIdentification = new PartyIdentificationType[] { new PartyIdentificationType { ID = new IDType { schemeID = Invoice.ShipmentVknNo, Value = Invoice.ShipmentPartyIdentification } } },
                            PartyName = new PartyNameType { Name = new NameType1 { Value = Invoice.ShipmentPartyName } },
                            PostalAddress = new AddressType
                            {
                                CitySubdivisionName = new CitySubdivisionNameType { Value = Invoice.ShipmentCitySubdivisionName },
                                CityName = new CityNameType { Value = Invoice.ShipmentCityName },
                                Country = new CountryType { Name = new NameType1 { Value = Invoice.ShipmentCountry } }
                            }
                        },
                        Despatch = new DespatchType
                        {
                            ActualDespatchDate = new ActualDespatchDateType { Value = Invoice.ShipmentActualDespatchDate },
                            ActualDespatchTime = new ActualDespatchTimeType { Value = Invoice.ShipmentActualDespatchTime }
                        }
                    },
                    TransportHandlingUnit = new TransportHandlingUnitType[]
                        {
                            new TransportHandlingUnitType
                            {
                                TransportEquipment = new TransportEquipmentType[]
                                {
                                    new TransportEquipmentType
                                    {
                                        ID = new IDType {schemeID =Invoice.ShipmentCarLicence, Value = Invoice.ShipmentCar }
                                    }
                                }
                            }
                        }
                },
                DespatchLine = GetDespatchLinesExportDespatch()
            };

            return eirsaliye;
        }

        private DespatchAdviceType CreateUBL_ForExportDespatch()
        {
            List<NoteType> despatchNotesList = new List<NoteType>();
            foreach (string DespatchNote in Invoice.Notes)
            {
                if (!String.IsNullOrEmpty(DespatchNote))
                {
                    NoteType noteType = new NoteType() { Value = DespatchNote };
                    despatchNotesList.Add(noteType);
                }
            }

            NoteType[] despatchNotes = despatchNotesList.ToArray();
            List<DocumentReferenceType> DespatchDocumentReferenceList;
            if (Invoice.Shipments.Count() > 0)
            {
                DespatchDocumentReferenceList = new List<DocumentReferenceType>();

                foreach (Shipment currShip in Invoice.Shipments)
                {
                    DocumentReferenceType currDRT = new DocumentReferenceType()
                    {
                        ID = new IDType()
                        {
                            Value = currShip.SalesShipmentNo
                        },
                        IssueDate = new IssueDateType
                        {
                            Value = currShip.SalesShipmentDate
                        }
                    };
                    DespatchDocumentReferenceList.Add(currDRT);
                }
            }
            else
            {
                DespatchDocumentReferenceList = null;
            }

            OrderReferenceType[] DespatchOrder = null;
            List<OrderReferenceType> OrderReferenceList = new List<OrderReferenceType>();

            foreach (Order curOrder in Invoice.Orders)
            {
                OrderReferenceType currOrderType = new OrderReferenceType()
                {
                    ID = new IDType()
                    {
                        Value = curOrder.OrderNo
                    },
                    IssueDate = new IssueDateType()
                    {
                        Value = curOrder.OrderDate
                    }
                };
                OrderReferenceList.Add(currOrderType);
            }
            DespatchOrder = OrderReferenceList.ToArray();

            IDType IDTag;
            if (string.IsNullOrEmpty(Invoice.IdTag))
            {
                IDTag = new IDType { Value = "" };
            }
            else
            {
                IDTag = new IDType { Value = Invoice.IdTag };
            }

            IDType AdditionalDocumentReferenceID;
            if (string.IsNullOrEmpty(Invoice.DespatchNumber))
            {
                AdditionalDocumentReferenceID = null;
            }
            else
            {
                AdditionalDocumentReferenceID = new IDType { Value = Invoice.DespatchNumber };
            }

            AttachmentType InvAttachment = null;
            if (string.IsNullOrEmpty(PdfFileName))
            {
                InvAttachment = null;
            }
            else
            {
                InvAttachment = new AttachmentType
                {
                    EmbeddedDocumentBinaryObject = new EmbeddedDocumentBinaryObjectType()
                    {
                        characterSetCode = "UTF-8",
                        filename = PdfFileName,
                        encodingCode = "Base64",
                        mimeCode = "application/pdf"
                    }
                };
            }

            DocumentReferenceType CatalogDocRefType = null;
            DocumentReferenceType ContractDocReftype = null;

            if (ShowContractAdditionalDocRef)
            {
                IDType AdditDocReferenceIDContract;
                if (string.IsNullOrEmpty(Invoice.ContractType))
                {
                    AdditDocReferenceIDContract = null;
                }
                else
                {
                    AdditDocReferenceIDContract = new IDType { Value = Invoice.ContractType };
                }


                ContractDocReftype = new DocumentReferenceType()
                {
                    ID = AdditDocReferenceIDContract,
                    IssueDate = new IssueDateType
                    {
                        Value = Invoice.ContractTypeDate
                    },
                    DocumentTypeCode = new DocumentTypeCodeType { Value = "Kontrat" }
                };
            }

            if (ShowCatalogTypeAdditDocRef)
            {
                IDType AdditionalDocumentReferencePaymenTypeID;
                if (string.IsNullOrEmpty(Invoice.CatalogType))
                {
                    AdditionalDocumentReferencePaymenTypeID = null;
                }
                else
                {
                    AdditionalDocumentReferencePaymenTypeID = new IDType { Value = Invoice.CatalogType };
                }

                CatalogDocRefType = new DocumentReferenceType()
                {
                    ID = AdditionalDocumentReferencePaymenTypeID,
                    IssueDate = new IssueDateType
                    {
                        Value = Invoice.CatalogTypeDate
                    },
                    DocumentTypeCode = new DocumentTypeCodeType { Value = "Katalog" }
                };
            }

            DocumentReferenceType[] AddDocRef;
            if (HideAdditDocRefference)
            {
                AddDocRef = null;
            }
            else
            {
                AddDocRef = new DocumentReferenceType[]
                {
                    new DocumentReferenceType()
                    {
                        ID = AdditionalDocumentReferenceID,

                        IssueDate = new IssueDateType
                        {
                            Value = Invoice.DespatchDateCust
                        },
                        DocumentTypeCode = new DocumentTypeCodeType { Value = "CUST_DES_ID" },
                        DocumentType = new DocumentTypeType {Value = "CUST_DES_ID" },
                        Attachment = InvAttachment
                    },
                    CatalogDocRefType,
                    ContractDocReftype
                };
            }

            List<PartyIdentificationType> SuplPartyIdentificationType = new List<PartyIdentificationType>();
            SuplPartyIdentificationType.Add(new PartyIdentificationType() { ID = new IDType { schemeID = Invoice.SupplierVknType, Value = Fit_Vkn } });
            if (!string.IsNullOrEmpty(SuplTicaretSicilNo))
            {
                SuplPartyIdentificationType.Add(new PartyIdentificationType() { ID = new IDType { schemeID = "TICARETSICILNO", Value = SuplTicaretSicilNo } });
            }
            if (!string.IsNullOrEmpty(SuplMersisNo))
            {
                SuplPartyIdentificationType.Add(new PartyIdentificationType() { ID = new IDType { schemeID = "MERSISNO", Value = SuplMersisNo } });
            }

            Type aa = Type.GetType("System.Xml.XmlElement");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<xml />");
            DespatchAdviceType eirsaliye = new DespatchAdviceType()
            {
                UBLVersionID = new UBLVersionIDType { Value = "2.1" }, //international standard of 2.1 bill
                CustomizationID = new CustomizationIDType { Value = "TR1.2.1" }, //but by using the IOP UBL naming Turkey as TR-specific invoice format 1.2.
                ProfileID = new ProfileIDType { Value = Invoice.DespatchType }, //There are two types of commercial and basic. response system in the commercial invoice (application response) returns.
                CopyIndicator = new CopyIndicatorType { Value = false }, //m copy is ascertained whether tenor
                UUID = new UUIDType { Value = Guid.NewGuid().ToString() }, //invoice ID
                ID = IDTag,
                IssueDate = new IssueDateType //Invoice Date
                {
                    Value = Invoice.DespatchDate
                },
                IssueTime = new IssueTimeType
                {
                    Value = Convert.ToDateTime(Invoice.PostingTime)
                },
                DespatchAdviceTypeCode = new DespatchAdviceTypeCodeType { Value = Invoice.DespatchTypeCode }, //invoice will be sent kinds of sales, returns, etc.
                Note = despatchNotes,
                LineCountNumeric = new LineCountNumericType { Value = Invoice.DespatchLines.Count }, //The number of invoice items

                OrderReference = DespatchOrder,
                AdditionalDocumentReference = AddDocRef,

                DespatchSupplierParty = new SupplierPartyType //information about sender of invoice
                {
                    Party = new PartyType()
                    {
                        WebsiteURI = new WebsiteURIType { Value = Invoice.SupplierWebSite },

                        PartyIdentification = SuplPartyIdentificationType.ToArray(),

                        PartyName = new PartyNameType { Name = new NameType1 { Value = Invoice.SupplierName } },

                        PostalAddress = new AddressType
                        {
                            ID = new IDType { Value = Invoice.SupplierRoom },
                            StreetName = new StreetNameType { Value = Invoice.SupplierAddress },
                            BuildingNumber = new BuildingNumberType { Value = Invoice.SupplierBuildingNumber },
                            CitySubdivisionName = new CitySubdivisionNameType { Value = Invoice.CustomerCitySubdivisionName },
                            CityName = new CityNameType { Value = Invoice.SupplierCity },
                            PostalZone = new PostalZoneType { Value = Invoice.SupplierPostalCode },
                            Region = new RegionType { Value = Invoice.SupplierRegion },
                            Country = new CountryType { Name = new NameType1 { Value = Invoice.SupplierCountry } }
                        },

                        PhysicalLocation = new LocationType1
                        {
                            ID = new IDType { Value = Invoice.SupplierTaxDepartment },
                            Address = new AddressType
                            {
                                ID = new IDType { Value = Invoice.SuppPhyId },
                                StreetName = new StreetNameType { Value = Invoice.SuppPhyStreetName },
                                BuildingNumber = new BuildingNumberType { Value = Invoice.SuppPhyBuildingNumber },
                                CitySubdivisionName = new CitySubdivisionNameType { Value = Invoice.SuppPhyCitySubDiv },
                                CityName = new CityNameType { Value = Invoice.SuppPhyCityName },
                                PostalZone = new PostalZoneType { Value = Invoice.SuppPhyPostalZone },
                                Country = new CountryType
                                {
                                    Name = new NameType1 { Value = Invoice.SuppPhyCountry }
                                }
                            }

                        },

                        PartyTaxScheme = new PartyTaxSchemeType
                        {
                            TaxScheme = new TaxSchemeType
                            {
                                Name = new NameType1 { Value = Invoice.SuppPhyTaxId }
                            }
                        },


                        Contact = new ContactType
                        {
                            Telephone = new TelephoneType { Value = Invoice.SupplierPhone },
                            Telefax = new TelefaxType { Value = Invoice.SupplierFax },
                            ElectronicMail = new ElectronicMailType { Value = Invoice.SupplierEmail }
                        },

                    }


                },

                DeliveryCustomerParty = new CustomerPartyType
                {
                    Party = new PartyType
                    {
                        WebsiteURI = new WebsiteURIType { Value = Invoice.DeliveryCustomerWebSite },

                        PartyIdentification = new PartyIdentificationType[]
                        {
                            new PartyIdentificationType()
                            {
                                ID = new IDType { schemeID = Invoice.DeliveryCustomerVknType, Value = Invoice.DeliveryCustomerVkn }
                            },
                            new PartyIdentificationType
                            {
                                ID = new IDType { schemeID = "MUSTERINO", Value = Invoice.DeliveryCustomerNo }
                            }
                        },

                        PartyName = new PartyNameType
                        {
                            Name = new NameType1 { Value = Invoice.DeliveryCustomerName }
                        },

                        PostalAddress = new AddressType
                        {
                            ID = new IDType { Value = Invoice.DeliveryCustomerId },
                            StreetName = new StreetNameType { Value = Invoice.DeliveryCustomerAddress },
                            BuildingNumber = new BuildingNumberType { Value = Invoice.DeliveryCustomerBuilNumber },
                            CitySubdivisionName = new CitySubdivisionNameType { Value = Invoice.DeliveryCustomerCitySub },
                            CityName = new CityNameType { Value = Invoice.DeliveryCustomerCity },
                            PostalZone = new PostalZoneType { Value = Invoice.DeliveryCustomerPostalCode },
                            Region = new RegionType { Value = Invoice.DeliveryCustomerRegion },
                            Country = new CountryType { Name = new NameType1 { Value = Invoice.DeliveryCustomerCountry } }
                        },

                        PartyTaxScheme = new PartyTaxSchemeType
                        {
                            TaxScheme = new TaxSchemeType { Name = new NameType1 { Value = Invoice.DeliveryCustomerTaxDepartment } }
                        },

                        Contact = new ContactType
                        {
                            Telephone = new TelephoneType { Value = Invoice.DeliveryCustomerPhone },
                            Telefax = new TelefaxType { Value = Invoice.DeliveryCustomerFax },
                            ElectronicMail = new ElectronicMailType { Value = Invoice.DeliveryCustomerEmail }
                        },

                    }
                },
                //BuyerCustomerParty = new CustomerPartyType
                //{
                //    Party = new PartyType
                //    {
                //        WebsiteURI = new WebsiteURIType { Value = invoice.BuyerCustomerWebSite },

                //        PartyIdentification = new PartyIdentificationType[]
                //        {
                //                new PartyIdentificationType()
                //                {
                //                    ID = new IDType { schemeID = invoice.BuyerCustomerVKNType, Value = invoice.BuyerCustomerVKN }
                //                },
                //                new PartyIdentificationType
                //                {
                //                    ID = new IDType { schemeID ="MUSTERINO", Value = invoice.DeliveryCustomerNo }
                //                }
                //        },

                //        PartyName = new PartyNameType
                //        {
                //            Name = new NameType1 { Value = invoice.DeliveryCustomerName }
                //        },

                //        PostalAddress = new AddressType
                //        {
                //            ID = new IDType { Value = invoice.BuyerCustomerID },
                //            StreetName = new StreetNameType { Value = invoice.BuyerCustomerAddress },
                //            BuildingNumber = new BuildingNumberType { Value = invoice.BuyerCustomerBuilNumber },
                //            CitySubdivisionName = new CitySubdivisionNameType { Value = invoice.BuyerCustomerCitySub },
                //            CityName = new CityNameType { Value = invoice.BuyerCustomerCity },
                //            PostalZone = new PostalZoneType { Value = invoice.BuyerCustomerPostalCode },
                //            Region = new RegionType { Value = invoice.BuyerCustomerRegion },
                //            Country = new CountryType { Name = new NameType1 { Value = invoice.BuyerCustomerCountry } }
                //        },

                //        PartyTaxScheme = new PartyTaxSchemeType
                //        {
                //            TaxScheme = new TaxSchemeType { Name = new NameType1 { Value = invoice.DeliveryCustomerTaxDepartment } }
                //        },

                //        Contact = new ContactType
                //        {
                //            Telephone = new TelephoneType { Value = invoice.DeliveryCustomerPhone },
                //            Telefax = new TelefaxType { Value = invoice.DeliveryCustomerFax },
                //            ElectronicMail = new ElectronicMailType { Value = invoice.DeliveryCustomerEmail }
                //        }

                //     }

                //},
                //SellerSupplierParty = new SupplierPartyType
                //{
                //    Party = new PartyType
                //    {
                //        WebsiteURI = new WebsiteURIType { Value = invoice.SellerCustomerWebSite },

                //        PartyIdentification = new PartyIdentificationType[]
                //        {
                //            new PartyIdentificationType()
                //            {
                //                ID = new IDType { schemeID = invoice.SellerCustomerVKNType, Value = invoice.SellerCustomerVKN }
                //            },
                //            new PartyIdentificationType
                //            {
                //                ID = new IDType { schemeID ="MUSTERINO", Value = invoice.SellerCustomerNo }
                //            }
                //        },

                //        PartyName = new PartyNameType
                //        {
                //            Name = new NameType1 { Value = invoice.SellerCustomerName }
                //        },

                //        PostalAddress = new AddressType
                //        {
                //            ID = new IDType { Value = invoice.SellerCustomerID },
                //            StreetName = new StreetNameType { Value = invoice.SellerCustomerAddress },
                //            BuildingNumber = new BuildingNumberType { Value = invoice.SellerCustomerBuilNumber },
                //            CitySubdivisionName = new CitySubdivisionNameType { Value = invoice.SellerCustomerCitySub },
                //            CityName = new CityNameType { Value = invoice.SellerCustomerCity },
                //            PostalZone = new PostalZoneType { Value = invoice.SellerCustomerPostalCode },
                //            Region = new RegionType { Value = invoice.SellerCustomerRegion },
                //            Country = new CountryType { Name = new NameType1 { Value = invoice.SellerCustomerCountry } }
                //        },

                //        PartyTaxScheme = new PartyTaxSchemeType
                //        {
                //            TaxScheme = new TaxSchemeType { Name = new NameType1 { Value = invoice.SellerCustomerTaxDepartment } }
                //        },

                //        Contact = new ContactType
                //        {
                //            Telephone = new TelephoneType { Value = invoice.SellerCustomerPhone },
                //            Telefax = new TelefaxType { Value = invoice.SellerCustomerFax },
                //            ElectronicMail = new ElectronicMailType { Value = invoice.SellerCustomerEmail }
                //        }

                //    }
                //},
                //OriginatorCustomerParty = new CustomerPartyType
                //{
                //    Party = new PartyType
                //    {
                //        WebsiteURI = new WebsiteURIType { Value = invoice.OrigCustomerWebSite },

                //        PartyIdentification = new PartyIdentificationType[]
                //        {
                //            new PartyIdentificationType()
                //            {
                //                ID = new IDType { schemeID = invoice.OrigCustomerVKNType, Value = invoice.OrigCustomerVKN }
                //            },
                //            new PartyIdentificationType
                //            {
                //                ID = new IDType { schemeID = "MUSTERINO", Value = invoice.OrigCustomerNo }
                //            }
                //        },

                //        PartyName = new PartyNameType
                //        {
                //            Name = new NameType1 { Value = invoice.OrigCustomerName }
                //        },

                //        PostalAddress = new AddressType
                //        {
                //            ID = new IDType { Value = invoice.OrigCustomerID },
                //            StreetName = new StreetNameType { Value = invoice.OrigCustomerAddress },
                //            BuildingName = new BuildingNameType { Value = invoice.OrigCustomerBuilNumber },
                //            CitySubdivisionName = new CitySubdivisionNameType { Value = invoice.OrigCustomerCitySub },
                //            CityName = new CityNameType { Value = invoice.OrigCustomerCity },
                //            PostalZone = new PostalZoneType { Value = invoice.OrigCustomerPostalCode },
                //            Region = new RegionType { Value = invoice.OrigCustomerRegion },
                //            Country = new CountryType { Name = new NameType1 { Value = invoice.OrigCustomerCountry } }
                //        },

                //        PartyTaxScheme = new PartyTaxSchemeType
                //        {
                //            TaxScheme = new TaxSchemeType { Name = new NameType1 { Value = invoice.OrigCustomerTaxDepartment } }
                //        },

                //        Contact = new ContactType
                //        {
                //            Telephone = new TelephoneType { Value = invoice.OrigCustomerPhone },
                //            Telefax = new TelefaxType { Value = invoice.OrigCustomerFax },
                //            ElectronicMail = new ElectronicMailType { Value = invoice.OrigCustomerEmail }
                //        }

                //    }
                //},

                Shipment = new ShipmentType
                {
                    ID = new IDType { Value = Invoice.ShipmentId },
                    GoodsItem = new GoodsItemType[]
                        {
                           new GoodsItemType
                            {
                            ValueAmount = new ValueAmountType {currencyID = Invoice.DespatchCurrencyCode ,Value=Invoice.ShipmentValueAmount}
                            }
                        },
                    ShipmentStage = new ShipmentStageType[]
                        {
                            new ShipmentStageType
                                {
                                TransportMeans = new TransportMeansType
                                {
                                    RoadTransport = new RoadTransportType {LicensePlateID = new LicensePlateIDType { schemeID="PLAKA",Value= Invoice.ShipmentLicensePlateId } }
                                },
                                DriverPerson = new PersonType[]
                                {
                                    new PersonType
                                    {
                                        FirstName = new FirstNameType { Value = Invoice.ShipmentFirstName },
                                        FamilyName = new FamilyNameType { Value = Invoice.ShipmentFamilyName},
                                        Title = new TitleType { Value = Invoice.ShipmentTitle },
                                        NationalityID = new NationalityIDType { Value = Invoice.ShipmentNationalityId }
                                    }
                                }
                            }
                        },
                    Delivery = new DeliveryType
                    {
                        CarrierParty = new PartyType
                        {
                            PartyIdentification = new PartyIdentificationType[] { new PartyIdentificationType { ID = new IDType { schemeID = Invoice.ShipmentVknNo, Value = Invoice.ShipmentPartyIdentification } } },
                            PartyName = new PartyNameType { Name = new NameType1 { Value = Invoice.ShipmentPartyName } },
                            PostalAddress = new AddressType
                            {
                                CitySubdivisionName = new CitySubdivisionNameType { Value = Invoice.ShipmentCitySubdivisionName },
                                CityName = new CityNameType { Value = Invoice.ShipmentCityName },
                                Country = new CountryType { Name = new NameType1 { Value = Invoice.ShipmentCountry } }
                            }
                        },
                        Despatch = new DespatchType
                        {
                            ActualDespatchDate = new ActualDespatchDateType { Value = Invoice.ShipmentActualDespatchDate },
                            ActualDespatchTime = new ActualDespatchTimeType { Value = Invoice.ShipmentActualDespatchTime }
                        }
                    },
                    TransportHandlingUnit = new TransportHandlingUnitType[]
                        {
                            new TransportHandlingUnitType
                            {
                                TransportEquipment = new TransportEquipmentType[]
                                {
                                    new TransportEquipmentType
                                    {
                                        ID = new IDType {schemeID = Invoice.ShipmentCarLicence, Value = Invoice.ShipmentCar }
                                    }
                                }
                            }
                        },

                },
                DespatchLine = GetDespatchLinesExportDespatch()
            };

            return eirsaliye;
        }

        public string IonicUNZipFile(byte[] zipData)
        {
            string filestr;
            string fileName;

            using (var stream = new MemoryStream(zipData))
            {
                using (ZipFile zout = ZipFile.Read(stream))
                {
                    ZipEntry entry = zout.FirstOrDefault();
                    using (var outStream = new MemoryStream())
                    {
                        entry.Extract(outStream);
                        fileName = entry.FileName;
                        outStream.Position = 0;
                        using (var sr = new StreamReader(outStream))
                        {
                            filestr = sr.ReadToEnd();

                        }
                    }
                }
            }
            return filestr;
        }

        public byte[] IonicUNZipFileByte(byte[] zipData)
        {
            using (var stream = new MemoryStream(zipData))
            {
                using (ZipFile zout = ZipFile.Read(stream))
                {
                    ZipEntry entry = zout.FirstOrDefault();
                    using (var outStream = new MemoryStream())
                    {
                        entry.Extract(outStream);
                        return outStream.ToArray();
                    }
                }
            }
        }

        public string SysIonicUNZipFile(byte[] zipData)
        {
            string xml;
            string fileName;

            using (var stream = new MemoryStream(SysResponce.Response[0].DocData))
            {
                using (ZipFile zout = ZipFile.Read(stream))
                {
                    ZipEntry entry = zout.FirstOrDefault();
                    using (var outStream = new MemoryStream())
                    {
                        entry.Extract(outStream);
                        fileName = entry.FileName;
                        outStream.Position = 0;
                        using (var sr = new StreamReader(outStream))
                        {
                            xml = sr.ReadToEnd();

                        }
                    }
                }
            }
            return xml;
        }

        public static T GetUbl<T>(string xml)
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(UserList));

            byte[] byteArray = Encoding.UTF8.GetBytes(xml);
            MemoryStream ms = new MemoryStream(byteArray);
            TextReader reader = new StreamReader(ms, Encoding.UTF8);

            var obj = (T)SerializerObj.Deserialize(reader);
            reader.Close();

            return obj;
        }

        public UserList[] GetApplicationResponseFromZipArray(byte[] zipArray)
        {
            var xml = this.IonicUNZipFile(zipArray);

            return FitService.GetUbl<UserList[]>(xml);
        }


        private static string GetXML<T>(T obj)
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(T));

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            ns.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.Add("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            ns.Add("", "urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2");
            ns.Add("xsi", "urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2 ../xsdrt/maindoc/UBL-DespatchAdvice-2.1.xsd");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");


            MemoryStream ms = new MemoryStream();
            TextWriter WriteFileStream = new StreamWriter(ms, Encoding.UTF8);

            SerializerObj.Serialize(WriteFileStream, obj, ns);


            WriteFileStream.Close();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private byte[] IonicZipFile(string xml, string fileName)
        {

            byte[] ziplenecekData = Encoding.UTF8.GetBytes(xml);

            MemoryStream zipStream = new MemoryStream();

            using (ZipFile zip = new Ionic.Zip.ZipFile())
            {
                ZipEntry zipEleman = zip.AddEntry(fileName + ".xml", ziplenecekData);

                zip.Save(zipStream);
            }

            zipStream.Seek(0, SeekOrigin.Begin);
            zipStream.Flush();

            zipStream.Position = 0;
            return zipStream.ToArray();
        }

        private bool ValidateXML(string inXML)
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(null, AppDomain.CurrentDomain.BaseDirectory + "/Add-ins/FITEDespatch/XSD/maindoc/UBL-DespatchAdvice-2_1.xsd");
            XDocument doc = XDocument.Parse(inXML.Substring(inXML.IndexOf(Environment.NewLine)));

            doc.Validate(schemas, (o, e) =>
            {
                throw new Exception(e.Message);
            });

            return true;
        }

        #endregion Send Despatch

        #region Receive Despatch

        public void GetDesReceiveInvoice(getDesUBLListRequest req)
        {
            DateTime Receivetime = DateTime.Now;
            ClientEDespatchServicesPortClient wsClient = CreateWSClient();
            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)wsClient.InnerChannel))
            {
                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add(HttpRequestHeader.Authorization, GetAuthorization());
                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer4 =
                       new System.Xml.Serialization.XmlSerializer(typeof(getDesUBLListRequest));
                    System.IO.StreamWriter file4 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "GetDesReceiveRequest.txt");
                    writer4.Serialize(file4, req);
                    file4.Close();

                }

                receive_response = wsClient.getDesUBLList(req);

                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer5 =
                       new System.Xml.Serialization.XmlSerializer(typeof(getDesUBLListResponse[]));
                    System.IO.StreamWriter file5 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "GetDesReceiveResponce.txt");
                    writer5.Serialize(file5, responce);
                    file5.Close();
                }
            }
        }

        public Int32 GetReceiveCount()
        {
            if (receive_response.Response != null)
            {
                return this.receive_response.Response.Count();
            }
            else
            {
                return 0;
            }
        }

        public GetDesUBLListResponseType GetReceive(Int32 Receivekey)
        {
            if (receive_response.Response != null)
            {
                return receive_response.Response[Receivekey];
            }
            else
            {
                return new GetDesUBLListResponseType();
            }
        }

        public string GetDesUbl(string identifier, string vkntckn, string uuid, string docType, string receiveType)
        {
            string[] UUIDArray = new string[1];
            UUIDArray[0] = uuid;
            getDesUBLRequest getDesUBLReq = new getDesUBLRequest();
            getDesUBLReq.Identifier = identifier;
            getDesUBLReq.VKN_TCKN = vkntckn;
            getDesUBLReq.UUID = UUIDArray;
            getDesUBLReq.DocType = docType;
            getDesUBLReq.Type = receiveType;

            ClientEDespatchServicesPortClient wsClient = CreateWSClient();
            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)wsClient.InnerChannel))
            {
                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add(HttpRequestHeader.Authorization, GetAuthorization());
                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer4 =
                       new System.Xml.Serialization.XmlSerializer(typeof(getDesUBLRequest));
                    System.IO.StreamWriter file4 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "GetDesUBLReq.txt");
                    writer4.Serialize(file4, getDesUBLReq);
                    file4.Close();
                }

                getDesUBLResponse Receive = wsClient.getDesUBL(getDesUBLReq);

                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer5 =
                       new System.Xml.Serialization.XmlSerializer(typeof(getDesUBLListResponse[]));
                    System.IO.StreamWriter file5 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "GetDesUBLRes.txt");
                    writer5.Serialize(file5, responce);
                    file5.Close();
                }
                var str = System.Text.Encoding.UTF8.GetString(Receive.Response[0].DocData);
                return str;
            }
        }

        public Stream GetDesInvoiceView(string identifier, string vkntckn, string uuid, string id, string receiveType, string docType, string viewType)
        {
            getDesViewRequest getDesUBLReq = new getDesViewRequest();
            getDesUBLReq.Identifier = identifier;
            getDesUBLReq.VKN_TCKN = vkntckn;
            getDesUBLReq.DocDetails = new GetDesViewRequestType[1];
            getDesUBLReq.DocDetails[0] = new GetDesViewRequestType();
            getDesUBLReq.DocDetails[0].UUID = uuid;
            getDesUBLReq.DocDetails[0].Type = receiveType;
            getDesUBLReq.DocDetails[0].DocType = docType;
            getDesUBLReq.DocDetails[0].ViewType = viewType;

            if (!uuid.Equals("") && !uuid.Equals(null))
            {
                getDesUBLReq.DocDetails[0].UUID = uuid;
            }
            if (!id.Equals("") && !id.Equals(null))
            {
                getDesUBLReq.DocDetails[0].ID = id;
            }

            ClientEDespatchServicesPortClient wsClient = CreateWSClient();
            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)wsClient.InnerChannel))
            {
                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add(HttpRequestHeader.Authorization, GetAuthorization());
                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer4 =
                       new System.Xml.Serialization.XmlSerializer(typeof(getDesUBLRequest));
                    System.IO.StreamWriter file4 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "GetDesReceiveUBLReq.txt");
                    writer4.Serialize(file4, getDesUBLReq);
                    file4.Close();
                }

                getDesViewResponse ReceiveView = wsClient.getDesView(getDesUBLReq);
                //File.WriteAllBytes("Despatch.pdf", ReceiveView.Response[0].DocData.ToArray());

                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer5 =
                       new System.Xml.Serialization.XmlSerializer(typeof(getDesUBLListResponse[]));
                    System.IO.StreamWriter file5 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "GetDesReceiveUBLRes.txt");
                    writer5.Serialize(file5, responce);
                    file5.Close();
                }
                var filebytes = IonicUNZipFileByte(ReceiveView.Response[0].DocData.ToArray());
                MemoryStream IncomingView = new MemoryStream(filebytes);
                return IncomingView;
            }
        }

        #endregion

        #region Uygulama Yanıtı
        public void IncomingApplicationResponce(getDesUBLListRequest req)
        {
            ClientEDespatchServicesPortClient wsClient = CreateWSClient();
            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)wsClient.InnerChannel))
            {
                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add(HttpRequestHeader.Authorization, GetAuthorization());
                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer4 =
                        new System.Xml.Serialization.XmlSerializer(typeof(getDesUBLListRequest));
                    System.IO.StreamWriter file4 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "getDesUBLListRequest.txt");
                    writer4.Serialize(file4, req);
                    file4.Close();
                }

                responceApp = wsClient.getDesUBLList(req);

                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer5 =
                        new System.Xml.Serialization.XmlSerializer(typeof(GetDesUBLListResponseType[]));
                    System.IO.StreamWriter file5 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "getDesUBLListResponce.xml");
                    writer5.Serialize(file5, responceApp);
                    file5.Close();
                }
            }
        }
        public Int32 GetApplicationResponceCount()
        {
            if (responceApp.Response != null)
            {
                return this.responceApp.Response.Count();
            }
            else
            {
                return 0;
            }
        }
        public GetDesUBLListResponseType GetResponseApp(Int32 key)
        {
            if (responceApp.Response != null)
            {
                return this.responceApp.Response[key];
            }
            else
            {
                return new GetDesUBLListResponseType();
            }
        }

        public string GetDesUblSystem(string identifier, string vkntckn, string uuid, string docType, string receiveType)
        {
            string[] UUIDArray = new string[1];
            UUIDArray[0] = uuid;
            getDesUBLRequest getDesUBLReq = new getDesUBLRequest();
            getDesUBLReq.Identifier = identifier;
            getDesUBLReq.VKN_TCKN = vkntckn;
            getDesUBLReq.UUID = UUIDArray;
            getDesUBLReq.DocType = docType;
            getDesUBLReq.Type = receiveType;

            ClientEDespatchServicesPortClient wsClient = CreateWSClient();
            using (new System.ServiceModel.OperationContextScope((System.ServiceModel.IClientChannel)wsClient.InnerChannel))
            {
                System.ServiceModel.Web.WebOperationContext.Current.OutgoingRequest.Headers.Add(HttpRequestHeader.Authorization, GetAuthorization());
                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer4 =
                       new System.Xml.Serialization.XmlSerializer(typeof(getDesUBLRequest));
                    System.IO.StreamWriter file4 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "SysGetDesUBLReq.txt");
                    writer4.Serialize(file4, getDesUBLReq);
                    file4.Close();
                }

                SysResponce = wsClient.getDesUBL(getDesUBLReq);

                if (DebugMode)
                {
                    System.Xml.Serialization.XmlSerializer writer5 =
                       new System.Xml.Serialization.XmlSerializer(typeof(getDesUBLListResponse[]));
                    System.IO.StreamWriter file5 = new System.IO.StreamWriter(
                        RequestandResponceLocation + "SysGetDesUBLRes.txt");
                    writer5.Serialize(file5, responce);
                    file5.Close();
                }

                var str = System.Text.Encoding.UTF8.GetString(SysResponce.Response[0].DocData);
                var xml = SysIonicUNZipFile(SysResponce.Response[0].DocData);
                MemoryStream tempS = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                StreamReader reader = new StreamReader(tempS);
                reader.Close();
                return str;
            }
        }

        #endregion
    }
}
