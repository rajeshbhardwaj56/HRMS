
using DocumentFormat.OpenXml.EMMA;
using HRMS.Models.Common;
using HRMS.Models.EmployeeOnboarding;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HRMS.Web.Controllers
{
    public class OnboardingController : Controller
    {
        private readonly IBusinessLayer _businessLayer;
        public OnboardingController(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> StartOnboarding(
            EmployeeOnboardingViewModel model)
        {
            try
            {
                // =========================================================
                // VALIDATE MODEL
                // =========================================================

                if (model == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid onboarding information."
                    });
                }


                // =========================================================
                // VALIDATE FULL NAME
                // =========================================================

                if (string.IsNullOrWhiteSpace(model.FullName))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please enter your full name."
                    });
                }


                // =========================================================
                // VALIDATE EMAIL
                // =========================================================

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please enter your email address."
                    });
                }


                // =========================================================
                // VALIDATE MOBILE
                // =========================================================

                if (string.IsNullOrWhiteSpace(model.Mobile))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please enter your mobile number."
                    });
                }


                string mobile = model.Mobile.Trim();


                if (!System.Text.RegularExpressions.Regex.IsMatch(
                        mobile,
                        @"^[6-9][0-9]{9}$"))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please enter a valid 10-digit mobile number."
                    });
                }


                // =========================================================
                // API URL
                // =========================================================

                var apiUrl =
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Onboarding,
                        APIApiActionConstants.StartOnboarding
                    );


                // =========================================================
                // CALL ONBOARDING API
                // =========================================================

                var response =
                    await _businessLayer.SendPostAPIRequest(
                        model,
                        apiUrl,
                        "",
                        false
                    );


                var data =
                    response?.ToString();


                // =========================================================
                // CHECK API RESPONSE
                // =========================================================

                if (string.IsNullOrWhiteSpace(data))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Onboarding API returned an empty response."
                    });
                }


                // =========================================================
                // LOG RAW RESPONSE
                // =========================================================

                System.Diagnostics.Debug.WriteLine(
                    "StartOnboarding API Response: " + data
                );


                // =========================================================
                // DESERIALIZE RESPONSE
                // =========================================================

                Result result;

                try
                {
                    result =
                        JsonConvert.DeserializeObject<Result>(data);
                }
                catch (Exception jsonEx)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Invalid response received from onboarding API: "
                            + jsonEx.Message
                    });
                }


                // =========================================================
                // CHECK RESULT
                // =========================================================

                if (result == null)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Invalid response received from onboarding API."
                    });
                }


                // =========================================================
                // CHECK ONBOARDING ID
                // =========================================================

                if (result.PKNo <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            string.IsNullOrWhiteSpace(result.Message)
                                ? "Onboarding record was not created."
                                : result.Message
                    });
                }


                // =========================================================
                // ONBOARDING ID
                // =========================================================

                long onboardingID =
                    result?.PKNo ?? 0;


                // =========================================================
                // CREATE PRIVATE KEY
                // =========================================================

                string privateKey =
                    _businessLayer.EncodeStringBase64( 
                        onboardingID.ToString()
                    );


                // =========================================================
                // EMPLOYMENT FORM URL
                // =========================================================

                string redirectUrl =
                    Url.Action(
                        "EmploymentForm",
                        "Onboarding",
                        new
                        {
                            key = privateKey
                        }
                    );


                // =========================================================
                // RETURN SUCCESS
                // =========================================================

                return Json(new
                {
                    success = true,

                    onboardingID = onboardingID,

                    privateKey = privateKey,

                    redirectUrl = redirectUrl,

                    message =
                        string.IsNullOrWhiteSpace(result.Message)
                            ? "Onboarding started successfully."
                            : result.Message
                });
            }
            catch (Exception ex)
            {
                // =========================================================
                // EXCEPTION
                // =========================================================

                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult EmploymentForm(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return RedirectToAction("Index", "Onboarding");
            }

            try
            {
                // =========================================================
                // DECODE KEY
                // =========================================================

                string decodedValue =
                    _businessLayer.DecodeStringBase64(key);

                if (!long.TryParse(decodedValue, out long onboardingID))
                {
                    return RedirectToAction("Index", "Onboarding");
                }

                if (onboardingID <= 0)
                {
                    return RedirectToAction("Index", "Onboarding");
                }


                // =========================================================
                // API REQUEST
                // =========================================================

                var request = new
                {
                    OnboardingID = onboardingID
                };


                // =========================================================
                // CALL API
                // =========================================================

                var response =
                    _businessLayer.SendPostAPIRequest(
                        request,

                        _businessLayer.GetFormattedAPIUrl(
                            APIControllarsConstants.Onboarding,
                            APIApiActionConstants.GetEmployeeDetailsOnboarding
                        ),

                        "",

                        false
                    )
                    .Result;

                var data = response?.ToString();


                // =========================================================
                // CHECK RESPONSE
                // =========================================================

                if (string.IsNullOrWhiteSpace(data))
                {
                    return RedirectToAction(
                        "Index",
                        "Onboarding");
                }


                // =========================================================
                // DESERIALIZE
                // =========================================================

                HRMS.Models.Common.Results result;

                try
                {
                    result =
                        JsonConvert.DeserializeObject<
                            HRMS.Models.Common.Results>(data);
                }
                catch
                {
                    return RedirectToAction(
                        "Index",
                        "Onboarding");
                }


                if (result == null)
                {
                    return RedirectToAction(
                        "Index",
                        "Onboarding");
                }


                // =========================================================
                // GET EMPLOYEE DETAILS
                // =========================================================

                EmployeeDetailsOnboardingModel employeeModel =
                    result.EmployeeDetailsOnboardingList?
                        .FirstOrDefault()
                    ?? new EmployeeDetailsOnboardingModel();


                employeeModel.OnboardingID = onboardingID;


                // =========================================================
                // GET LAST SAVED STEP
                // =========================================================

                int lastSavedStep =
                    employeeModel.LastSavedStep;


                if (lastSavedStep < 1)
                {
                    lastSavedStep = 1;
                }

                if (lastSavedStep > 8)
                {
                    lastSavedStep = 8;
                }


                // =========================================================
                // CREATE PAGE MODEL
                // =========================================================

                var model =
                    new EmployeeOnboardingPageViewModel
                    {
                        EmployeeDetails =
                            employeeModel,

                        FamilyList =
                            result.EmployeeOnboardingFamilyList
                            ?? new List<EmployeeOnboardingFamilyModel>(),

                        EducationList =
                            result.EmployeeOnboardingEducationList
                            ?? new List<EmployeeOnboardingEducationModel>(),

                        EmploymentList =
                            result.EmployeeOnboardingEmploymentList
                            ?? new List<EmployeeOnboardingEmploymentModel>(),

                        BankDetailsList =
                            result.EmployeeOnboardingBankDetailsList
                            ?? new List<EmployeeOnboardingBankDetailsModel>(),

                        DocumnetList =
                            result.EmployeeOnboardingDocumentsList
                            ?? new List<EmployeeOnboardingDocumentsModel>(),

                        // =====================================================
                        // NEW - NOMINEES
                        // =====================================================

                        EPFNomineeList = result.EmployeeOnboardingNomineeList?
                    .Where(x => x.NomineeType == "EPF")
                    .ToList()
                    ?? new List<EmployeeOnboardingNomineeModel>(),

                                        EPSNomineeList = result.EmployeeOnboardingNomineeList?
                    .Where(x => x.NomineeType == "EPS")
                    .ToList()
                    ?? new List<EmployeeOnboardingNomineeModel>(),

                                        GratuityNomineeList = result.EmployeeOnboardingNomineeList?
                    .Where(x => x.NomineeType == "Gratuity")
                    .ToList()
                    ?? new List<EmployeeOnboardingNomineeModel>(),

                        // =====================================================
                        // NEW - INTERVIEW EVALUATION
                        // =====================================================

                        InterviewEvaluationList =
                            result.EmployeeOnboardingInterviewEvaluationList
                            ?? new List<EmployeeOnboardingInterviewEvaluationModel>(),

                        OnboardingID =
                            onboardingID,

                        Key =
                            key,

                        LastSavedStep =
                            lastSavedStep,

                        CurrentStep =
                            lastSavedStep
                    };


                // =========================================================
                // VIEWBAG
                // =========================================================

                ViewBag.CurrentStep =
                    lastSavedStep;

                ViewBag.LastSavedStep =
                    lastSavedStep;


                // =========================================================
                // RETURN VIEW
                // =========================================================

                return View(
                    "EmploymentForm",
                    model);
            }
            catch
            {
                return RedirectToAction(
                    "Index",
                    "Onboarding");
            }
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AddUpdateEmployeeDetailsOnboarding(
            EmployeeDetailsOnboardingModel model)
        {
            try
            {
                // =========================================================
                // CHECK MODEL
                // =========================================================

                if (model == null)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Employee onboarding details are required."
                    });
                }


                if (model.OnboardingID <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Invalid onboarding information."
                    });
                }


                // =========================================================
                // VALIDATE STEP
                // =========================================================

                if (model.CurrentStep < 1 ||
                    model.CurrentStep > 8)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Invalid onboarding step."
                    });
                }


                // =========================================================
                // INITIALIZE LISTS
                // =========================================================
                model.DocumentsList ??=
                    new List<EmployeeOnboardingDocumentsModel>();

                model.BankDetailsList ??=
                    new List<EmployeeOnboardingBankDetailsModel>();

                model.EducationList ??=
                    new List<EmployeeOnboardingEducationModel>();

                model.FamilyList ??=
                    new List<EmployeeOnboardingFamilyModel>();

                model.EmploymentList ??=
                    new List<EmployeeOnboardingEmploymentModel>();

                model.EPFNomineeList ??=
                    new List<EmployeeOnboardingNomineeModel>();

                model.EPSNomineeList ??=
                    new List<EmployeeOnboardingNomineeModel>();

                model.GratuityNomineeList ??=
                    new List<EmployeeOnboardingNomineeModel>();

                model.InterviewEvaluationList ??=
                    new List<EmployeeOnboardingInterviewEvaluationModel>();


                // =========================================================
                // CREATE UPLOAD FOLDER
                // =========================================================

                string uploadFolder =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "employee-onboarding",
                        model.OnboardingID.ToString()
                    );


                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }


                // =========================================================
                // SAVE EMPLOYEE PHOTO
                // =========================================================

                if (model.EmployeePhotoFile != null &&
                    model.EmployeePhotoFile.Length > 0)
                {
                    string[] allowedPhotoExtensions =
                    {
                ".jpg",
                ".jpeg",
                ".png"
            };


                    string extension =
                        Path.GetExtension(
                            model.EmployeePhotoFile.FileName)
                        .ToLowerInvariant();


                    if (!allowedPhotoExtensions.Contains(extension))
                    {
                        return Json(new
                        {
                            success = false,
                            message =
                                "Only JPG, JPEG and PNG images are allowed for employee photo."
                        });
                    }


                    if (model.EmployeePhotoFile.Length >
                        2 * 1024 * 1024)
                    {
                        return Json(new
                        {
                            success = false,
                            message =
                                "Employee photo size cannot exceed 2 MB."
                        });
                    }


                    string fileName =
                        $"EmployeePhoto_{Guid.NewGuid():N}{extension}";


                    string physicalPath =
                        Path.Combine(
                            uploadFolder,
                            fileName);


                    await using (var stream =
                        new FileStream(
                            physicalPath,
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.None))
                    {
                        await model.EmployeePhotoFile
                            .CopyToAsync(stream);
                    }


                    model.EmployeePhoto =
                        $"/uploads/employee-onboarding/" +
                        $"{model.OnboardingID}/{fileName}";
                }


                // =========================================================
                // HELPER - SAVE DOCUMENT
                // =========================================================

                async Task<string?> SaveDocument(
                    IFormFile? file,
                    string documentType,
                    long? documentReferenceID = null)
                {
                    if (file == null ||
                        file.Length <= 0)
                    {
                        return null;
                    }


                    // -----------------------------------------------------
                    // ALLOWED FILE TYPES
                    // -----------------------------------------------------

                    string[] allowedExtensions =
                    {
                ".pdf",
                ".jpg",
                ".jpeg",
                ".png"
            };


                    string extension =
                        Path.GetExtension(
                            file.FileName)
                        .ToLowerInvariant();


                    if (!allowedExtensions.Contains(extension))
                    {
                        throw new Exception(
                            $"Invalid file format for {documentType}. " +
                            "Only PDF, JPG, JPEG and PNG files are allowed."
                        );
                    }


                    // -----------------------------------------------------
                    // MAX 5 MB
                    // -----------------------------------------------------

                    if (file.Length >
                        5 * 1024 * 1024)
                    {
                        throw new Exception(
                            $"{documentType} file size cannot exceed 5 MB."
                        );
                    }


                    // -----------------------------------------------------
                    // SAFE DOCUMENT TYPE
                    // -----------------------------------------------------

                    string safeDocumentType =
                        new string(
                            documentType
                                .Where(c =>
                                    char.IsLetterOrDigit(c) ||
                                    c == '_' ||
                                    c == '-')
                                .ToArray()
                        );


                    if (string.IsNullOrWhiteSpace(
                        safeDocumentType))
                    {
                        safeDocumentType =
                            "Document";
                    }


                    // -----------------------------------------------------
                    // UNIQUE FILE NAME
                    // -----------------------------------------------------

                    string fileName =
                        $"{safeDocumentType}_" +
                        $"{Guid.NewGuid():N}" +
                        $"{extension}";


                    string physicalPath =
                        Path.Combine(
                            uploadFolder,
                            fileName);


                    // -----------------------------------------------------
                    // SAVE PHYSICAL FILE
                    // -----------------------------------------------------

                    await using (var stream =
                        new FileStream(
                            physicalPath,
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.None))
                    {
                        await file.CopyToAsync(stream);
                    }


                    // -----------------------------------------------------
                    // WEB PATH
                    // -----------------------------------------------------

                    string webPath =
                        $"/uploads/employee-onboarding/" +
                        $"{model.OnboardingID}/{fileName}";


                    // -----------------------------------------------------
                    // ADD DOCUMENT TO DOCUMENT LIST
                    // -----------------------------------------------------

                    model.DocumentsList.Add(
                        new EmployeeOnboardingDocumentsModel
                        {
                            DocumentID = 0,

                            OnboardingID =
                                model.OnboardingID,

                            DocumentType =
                                documentType,

                            FileName =
                                Path.GetFileName(
                                    file.FileName),

                            FilePath =
                                webPath,

                            UploadedAt =
                                DateTime.Now,

                            DocumentReferenceID =
                                documentReferenceID
                        });


                    return webPath;
                }


                // =========================================================
                // STEP 3 - EDUCATION DOCUMENTS
                // =========================================================

                if (model.CurrentStep == 3 &&
                    model.EducationList != null &&
                    model.EducationList.Count > 0)
                {
                    for (int i = 0;
                         i < model.EducationList.Count;
                         i++)
                    {
                        var education =
                            model.EducationList[i];


                        // -------------------------------------------------
                        // ROW NUMBER
                        // -------------------------------------------------

                        education.RowNo =
                            i + 1;


                        // -------------------------------------------------
                        // NO FILE
                        // -------------------------------------------------

                        if (education.DocumentFile == null ||
                            education.DocumentFile.Length <= 0)
                        {
                            continue;
                        }


                        // -------------------------------------------------
                        // DOCUMENT TYPE
                        // -------------------------------------------------

                        string documentType =
                            string.IsNullOrWhiteSpace(
                                education.DocumentType)
                            ? "EducationCertificate"
                            : education.DocumentType.Trim();


                        // -------------------------------------------------
                        // EDUCATION ID
                        // -------------------------------------------------

                        long? educationID = null;

                        if (education.EducationID > 0)
                        {
                            educationID =
                                education.EducationID;
                        }


                        // -------------------------------------------------
                        // SAVE DOCUMENT
                        // -------------------------------------------------

                        await SaveDocument(
                            education.DocumentFile,
                            documentType,
                            educationID);


                        // -------------------------------------------------
                        // DO NOT SEND IFORMFILE TO API
                        // -------------------------------------------------

                        education.DocumentFile =
                            null;
                    }
                }


                // =========================================================
                // STEP 5 - BANK DOCUMENT
                // =========================================================

                if (model.CurrentStep == 5)
                {
                    if (model.BankDocumentFile != null &&
                        model.BankDocumentFile.Length > 0)
                    {
                        string? bankDocumentPath =
                            await SaveDocument(
                                model.BankDocumentFile,
                                "BankDocument"
                            );


                        if (!string.IsNullOrWhiteSpace(
                            bankDocumentPath))
                        {
                            if (model.BankDetailsList.Count == 0)
                            {
                                model.BankDetailsList.Add(
                                    new EmployeeOnboardingBankDetailsModel
                                    {
                                        BankDetailID = 0,

                                        OnboardingID =
                                            model.OnboardingID
                                    }
                                );
                            }


                            model.BankDetailsList[0]
                                .BankCopyFile =
                                bankDocumentPath;
                        }
                    }
                }


                // =========================================================
                // STEP 7 - INTERVIEW / APPROVAL DOCUMENTS
                // =========================================================

                // =========================================================
                // STEP 7 - INTERVIEW / APPROVAL DOCUMENTS
                // =========================================================

                if (model.CurrentStep == 7)
                {
                    // ---------------------------------------------------------
                    // HR INTERVIEWER SIGNATURE
                    // ---------------------------------------------------------

                    await SaveDocument(
                        model.HRInterviewerSignatureFile,
                        "HRInterviewerSignature"
                    );


                    // ---------------------------------------------------------
                    // OPERATIONS INTERVIEWER SIGNATURE
                    // ---------------------------------------------------------

                    await SaveDocument(
                        model.OperationsInterviewerSignatureFile,
                        "OperationsInterviewerSignature"
                    );


                    // ---------------------------------------------------------
                    // PROCESS OWNER INTERVIEWER SIGNATURE
                    // ---------------------------------------------------------

                    await SaveDocument(
                        model.ProcessOwnerInterviewerSignatureFile,
                        "ProcessOwnerInterviewerSignature"
                    );


                    // ---------------------------------------------------------
                    // MAIN INTERVIEWER SIGNATURE
                    // ---------------------------------------------------------

                    string? interviewerSignaturePath =
                        await SaveDocument(
                            model.InterviewerSignatureFile,
                            "InterviewerSignature"
                        );


                    // ---------------------------------------------------------
                    // CANDIDATE / EMPLOYEE SIGNATURE
                    // ---------------------------------------------------------

                    string? signaturePath =
                        await SaveDocument(
                            model.SignatureFile,
                            "Signature"
                        );


                    // ---------------------------------------------------------
                    // FUNCTIONAL HEAD APPROVAL
                    // ---------------------------------------------------------

                    string? functionalHeadApprovalPath =
                        await SaveDocument(
                            model.FunctionalHeadApprovalFile,
                            "FunctionalHeadApproval"
                        );


                    // ---------------------------------------------------------
                    // SPOC HUMAN RESOURCES APPROVAL
                    // ---------------------------------------------------------

                    string? spocHRApprovalPath =
                        await SaveDocument(
                            model.SPOCHumanResourcesApprovalFile,
                            "SPOCHumanResourcesApproval"
                        );


                    // ---------------------------------------------------------
                    // HEAD HUMAN RESOURCES APPROVAL
                    // ---------------------------------------------------------

                    string? headHRApprovalPath =
                        await SaveDocument(
                            model.HeadHumanResourcesApprovalFile,
                            "HeadHumanResourcesApproval"
                        );


                    // ---------------------------------------------------------
                    // MAP INTERVIEWER SIGNATURE TO INTERVIEW EVALUATION
                    // ---------------------------------------------------------

                    if (model.InterviewEvaluationList != null &&
                        model.InterviewEvaluationList.Count > 0)
                    {
                        var interviewEvaluation =
                            model.InterviewEvaluationList[0];


                        if (!string.IsNullOrWhiteSpace(
                            interviewerSignaturePath))
                        {
                            interviewEvaluation.InterviewerSignature =
                                interviewerSignaturePath;
                        }


                        if (!string.IsNullOrWhiteSpace(
                            signaturePath))
                        {
                            interviewEvaluation.Signature =
                                signaturePath;
                        }


                        if (!string.IsNullOrWhiteSpace(
                            functionalHeadApprovalPath))
                        {
                            interviewEvaluation.FunctionalHeadApproval =
                                functionalHeadApprovalPath;
                        }


                        if (!string.IsNullOrWhiteSpace(
                            spocHRApprovalPath))
                        {
                            interviewEvaluation.SPOCHumanResourcesApproval =
                                spocHRApprovalPath;
                        }


                        if (!string.IsNullOrWhiteSpace(
                            headHRApprovalPath))
                        {
                            interviewEvaluation.HeadHumanResourcesApproval =
                                headHRApprovalPath;
                        }
                    }
                }
                // =========================================================
                // STEP 8 - IDENTITY DOCUMENTS + EMPLOYEE SIGNATURE
                // SAVE ALL FILES INTO DOCUMENTS TABLE
                // =========================================================

                if (model.CurrentStep == 8)
                {
                    // =========================================================
                    // PASSPORT
                    // =========================================================

                    if (model.PassportFile != null &&
                        model.PassportFile.Length > 0)
                    {
                        string? passportPath = await SaveDocument(
                            model.PassportFile,
                            "Passport"
                        );

                        // Optional - keep path in model for UI
                        model.PassportDocumentPath = passportPath;
                    }


                    // =========================================================
                    // PAN CARD
                    // =========================================================

                    if (model.PANFile != null &&
                        model.PANFile.Length > 0)
                    {
                        string? panPath = await SaveDocument(
                            model.PANFile,
                            "PAN"
                        );

                        // Optional - keep path in model for UI
                        model.PANDocumentPath = panPath;
                    }


                    // =========================================================
                    // AADHAAR CARD
                    // =========================================================

                    if (model.AadhaarFile != null &&
                        model.AadhaarFile.Length > 0)
                    {
                        string? aadhaarPath = await SaveDocument(
                            model.AadhaarFile,
                            "Aadhaar"
                        );

                        // Optional - keep path in model for UI
                        model.AadhaarDocumentPath = aadhaarPath;
                    }


                    // =========================================================
                    // EMPLOYEE SIGNATURE
                    // =========================================================

                    if (model.EmployeeSignatureFile != null &&
                        model.EmployeeSignatureFile.Length > 0)
                    {
                        string? employeeSignaturePath = await SaveDocument(
                            model.EmployeeSignatureFile,
                            "EmployeeSignature"
                        );

                        // Optional - keep path in model for UI
                        model.EmployeeSignaturePath = employeeSignaturePath;
                    }
                }

                // =========================================================
                // DOCUMENT COUNT
                // =========================================================

                int documentsSaved =
                    model.DocumentsList?.Count ?? 0;


                // =========================================================
                // REMOVE IFORMFILE PROPERTIES
                // =========================================================

                model.EmployeePhotoFile = null;

                model.BankDocumentFile = null;

                model.HRInterviewerSignatureFile = null;

                model.OperationsInterviewerSignatureFile = null;

                model.ProcessOwnerInterviewerSignatureFile = null;

                model.InterviewerSignatureFile = null;

                model.SignatureFile = null;

                model.FunctionalHeadApprovalFile = null;

                model.SPOCHumanResourcesApprovalFile = null;

                model.HeadHumanResourcesApprovalFile = null;
                model.PassportFile = null;
                model.PANFile = null;
                model.AadhaarFile = null;
                model.EmployeeSignatureFile = null;

                // =========================================================
                // CLEAR EDUCATION IFORMFILE
                // =========================================================

                if (model.EducationList != null)
                {
                    foreach (var education in
                             model.EducationList)
                    {
                        education.DocumentFile =
                            null;
                    }
                }



                // =========================================================
                // CALL ONBOARDING API
                // =========================================================

                var apiUrl =
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Onboarding,
                        APIApiActionConstants
                            .AddUpdateEmployeeDetailsOnboarding
                    );


                var response =
                    await _businessLayer.SendPostAPIRequest(
                        model,
                        apiUrl,
                        "",
                        false
                    );


                // =========================================================
                // READ API RESPONSE
                // =========================================================

                var data =
                    response?.ToString();


                if (string.IsNullOrWhiteSpace(data))
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Unable to save onboarding details."
                    });
                }


                Newtonsoft.Json.Linq.JObject result;

                try
                {
                    result =
                        Newtonsoft.Json.Linq.JObject.Parse(
                            data);
                }
                catch
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Invalid response received from onboarding API."
                    });
                }


                // =========================================================
                // CHECK API ERROR
                // =========================================================

                string? errorCode =
                    result["errorCode"]?.ToString();


                bool success =
                    string.IsNullOrWhiteSpace(
                        errorCode);


                if (!success)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            result["message"]?.ToString()
                            ?? "Unable to save onboarding details."
                    });
                }


                // =========================================================
                // GET LAST SAVED STEP FROM API
                // =========================================================
                //
                // IMPORTANT:
                // The database uses MAX(CurrentStep) logic through
                // LastSavedStep. Therefore do NOT simply return
                // model.CurrentStep here.
                // =========================================================

                int currentStep =
                    model.CurrentStep;


                int lastSavedStep =
                    result["lastSavedStep"] != null &&
                    result["lastSavedStep"].Type !=
                        Newtonsoft.Json.Linq.JTokenType.Null
                        ? result["lastSavedStep"]!.Value<int>()
                        : currentStep;


                // Safety
                if (lastSavedStep < 1)
                {
                    lastSavedStep = 1;
                }


                if (lastSavedStep > 8)
                {
                    lastSavedStep = 8;
                }


                // =========================================================
                // NEXT STEP
                // =========================================================

                int nextStep =
                    currentStep < 8
                        ? currentStep + 1
                        : 8;


                // =========================================================
                // SUCCESS
                // =========================================================

                return Json(new
                {
                    success = true,

                    message =
                        $"Step {currentStep} saved successfully.",

                    onboardingID =
                        model.OnboardingID,

                    currentStep =
                        currentStep,

                    // IMPORTANT:
                    // This must come from DB/API and not blindly
                    // from CurrentStep.
                    lastSavedStep =
                        lastSavedStep,

                    nextStep =
                        nextStep,

                    isFinalStep =
                        currentStep == 8,

                    documentsSaved =
                        documentsSaved
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ViewEmploymentForm(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return RedirectToAction("Index", "Onboarding");
            }

            try
            {
                // =========================================================
                // DECODE KEY
                // =========================================================

                string decodedValue =
                    _businessLayer.DecodeStringBase64(key);

                if (!long.TryParse(decodedValue, out long onboardingID))
                {
                    return RedirectToAction("Index", "Onboarding");
                }

                if (onboardingID <= 0)
                {
                    return RedirectToAction("Index", "Onboarding");
                }


                // =========================================================
                // API REQUEST
                // =========================================================

                var request = new
                {
                    OnboardingID = onboardingID
                };


                // =========================================================
                // CALL API
                // =========================================================

                var response =
                    _businessLayer.SendPostAPIRequest(
                        request,
                        _businessLayer.GetFormattedAPIUrl(
                            APIControllarsConstants.Onboarding,
                            APIApiActionConstants.GetEmployeeDetailsOnboarding
                        ),
                        "",
                        false
                    ).Result;


                var data = response?.ToString();


                // =========================================================
                // CHECK RESPONSE
                // =========================================================

                if (string.IsNullOrWhiteSpace(data))
                {
                    return RedirectToAction(
                        "Index",
                        "Onboarding");
                }


                // =========================================================
                // DESERIALIZE
                // =========================================================

                HRMS.Models.Common.Results result;

                try
                {
                    result =
                        JsonConvert.DeserializeObject<
                            HRMS.Models.Common.Results>(data);
                }
                catch
                {
                    return RedirectToAction(
                        "Index",
                        "Onboarding");
                }


                if (result == null)
                {
                    return RedirectToAction(
                        "Index",
                        "Onboarding");
                }


                // =========================================================
                // GET EMPLOYEE DETAILS
                // =========================================================

                EmployeeDetailsOnboardingModel employeeModel =
                    result.EmployeeDetailsOnboardingList?
                        .FirstOrDefault()
                    ?? new EmployeeDetailsOnboardingModel();

                employeeModel.OnboardingID = onboardingID;


                // =========================================================
                // CREATE VIEW MODEL
                // =========================================================

                var model =
                    new EmployeeOnboardingPageViewModel
                    {
                        EmployeeDetails =
                            employeeModel,

                        FamilyList =
                            result.EmployeeOnboardingFamilyList
                            ?? new List<EmployeeOnboardingFamilyModel>(),

                        EducationList =
                            result.EmployeeOnboardingEducationList
                            ?? new List<EmployeeOnboardingEducationModel>(),

                        EmploymentList =
                            result.EmployeeOnboardingEmploymentList
                            ?? new List<EmployeeOnboardingEmploymentModel>(),

                        BankDetailsList =
                            result.EmployeeOnboardingBankDetailsList
                            ?? new List<EmployeeOnboardingBankDetailsModel>(),

                        DocumnetList =
                            result.EmployeeOnboardingDocumentsList
                            ?? new List<EmployeeOnboardingDocumentsModel>(),


                        // =====================================================
                        // NOMINEES
                        // =====================================================

                        EPFNomineeList =
                            result.EmployeeOnboardingNomineeList?
                                .Where(x => x.NomineeType == "EPF")
                                .ToList()
                            ?? new List<EmployeeOnboardingNomineeModel>(),

                        EPSNomineeList =
                            result.EmployeeOnboardingNomineeList?
                                .Where(x => x.NomineeType == "EPS")
                                .ToList()
                            ?? new List<EmployeeOnboardingNomineeModel>(),

                        GratuityNomineeList =
                            result.EmployeeOnboardingNomineeList?
                                .Where(x => x.NomineeType == "Gratuity")
                                .ToList()
                            ?? new List<EmployeeOnboardingNomineeModel>(),


                        // =====================================================
                        // INTERVIEW EVALUATION
                        // =====================================================

                        InterviewEvaluationList =
                            result.EmployeeOnboardingInterviewEvaluationList
                            ?? new List<EmployeeOnboardingInterviewEvaluationModel>(),


                        // =====================================================
                        // COMMON
                        // =====================================================

                        OnboardingID = onboardingID,

                        Key = key,

                        LastSavedStep = 8,

                        CurrentStep = 1
                    };


                // =========================================================
                // VIEW MODE
                // =========================================================

                ViewBag.IsViewMode = true;

                ViewBag.OnboardingID = onboardingID;

                ViewBag.Key = key;


                // =========================================================
                // RETURN VIEW
                // =========================================================

                return View(
                    "ViewEmploymentForm",
                    model);
            }
            catch
            {
                return RedirectToAction(
                    "Index",
                    "Onboarding");
            }
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult EmployeeUndertaking(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return RedirectToAction("Index", "Onboarding");
            }

            try
            {
                string decodedValue = _businessLayer.DecodeStringBase64(key);

                if (!long.TryParse(decodedValue, out long onboardingID) ||
                    onboardingID <= 0)
                {
                    return RedirectToAction("Index", "Onboarding");
                }

                var model = new EmployeeUndertakingModel
                {
                    OnboardingID = onboardingID
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log ex
                return RedirectToAction("Index", "Onboarding");
            }
        }
        [HttpGet]
        public IActionResult OnboardingRequests()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetEmployeeOnboardingRequestList(
        string sEcho,
        int iDisplayStart,
        int iDisplayLength,
        string sSearch,
        string sortCol,
        string sortDir,
        string? status = null)
        {
            try
            {
// ============================================================
// COLUMN MAPPING
// ============================================================


    var columnMapping = new Dictionary<string, string>
    {
        { "onboardingID", "OnboardingID" },
        { "empCode", "EmpCode" },
        { "employeeName", "EmployeeName" },
        { "email", "Email" },
        { "mobile", "Mobile" },
        { "submittedDate", "SubmittedDate" },
        { "statusName", "Status" },
        { "status", "Status" }
    };

                // ============================================================
                // MODEL
                // ============================================================

                var model = new EmployeeOnboardingRequestInputParams
                {
                    Search = string.IsNullOrWhiteSpace(sSearch)
                        ? null
                        : sSearch.Trim(),

                    Status = status,

                    PageNumber = iDisplayLength > 0
                        ? (iDisplayStart / iDisplayLength) + 1
                        : 1,

                    PageSize = iDisplayLength > 0
                        ? iDisplayLength
                        : 10,

                    SortColumn =
                        !string.IsNullOrWhiteSpace(sortCol) &&
                        columnMapping.ContainsKey(sortCol)
                            ? columnMapping[sortCol]
                            : "SubmittedDate",

                    SortDirection =
                        string.IsNullOrWhiteSpace(sortDir)
                            ? "DESC"
                            : sortDir.ToUpper()
                };

                // ============================================================
                // API CALL
                // ============================================================

                var token = HttpContext.Session.GetString(
                    Constants.SessionBearerToken
                );

                var apiUrl = _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Onboarding,
                    APIApiActionConstants.GetEmployeeOnboardingRequests
                );

                var apiResponse = _businessLayer.SendPostAPIRequest(
                    model,
                    apiUrl,
                    token,
                    true
                ).Result;

                // ============================================================
                // CHECK API RESPONSE
                // ============================================================

                if (apiResponse == null)
                {
                    return Json(new
                    {
                        draw = sEcho,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>()
                    });
                }

                // ============================================================
                // DESERIALIZE
                // ============================================================

                var onboardingData =
                    JsonConvert.DeserializeObject<EmployeeOnboardingRequestResults>(
                        apiResponse.ToString()
                    );

                if (onboardingData == null)
                {
                    return Json(new
                    {
                        draw = sEcho,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>()
                    });
                }

                // ============================================================
                // DATATABLE RESPONSE
                // ============================================================

                return Json(new
                {
                    draw = sEcho,

                    recordsTotal = onboardingData.TotalRecords,

                    recordsFiltered = onboardingData.TotalRecords,

                    data = onboardingData.EmployeeOnboardingRequests
                        .Select(a => new
                        {
                            // =================================================
                            // BASIC DATA
                            // =================================================

                            onboardingID = a.OnboardingID,

                            empCode = a.EmpCode,

                            employeeName = a.EmployeeName,

                            email = a.Email,

                            mobile = a.Mobile,

                            employeeID = a.EmployeeID,

                            currentStep = a.CurrentStep,

                            lastSavedStep = a.LastSavedStep,

                            status = a.Status,

                            statusName = a.StatusName,

                            isSubmitted = a.IsSubmitted,

                            createdDate = a.CreatedDate,

                            modifiedDate = a.ModifiedDate,

                            submittedDate = a.SubmittedDate,


                            // =================================================
                            // ENCODED PRIVATE KEY
                            // =================================================
                            // Same logic:
                            //
                            // _businessLayer.EncodeStringBase64(
                            //     onboardingID.ToString()
                            // )
                            //
                            // This key will be used by the View and
                            // Interview buttons.
                            // =================================================

                            privateKey =
                                _businessLayer.EncodeStringBase64(
                                    a.OnboardingID.ToString()
                                )
                        })
                });
            }
            catch (Exception ex)
            {
                // ============================================================
                // TEMPORARY DEBUGGING
                // ============================================================

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }


        }


// =========================================================
// APPROVE MULTIPLE EMPLOYEE ONBOARDING REQUESTS
// =========================================================

[HttpPost]
public async Task<IActionResult> ApproveEmployeeOnboarding(
    [FromBody] List<string> keys)
        {
            try
            {
                // =========================================================
                // CHECK REQUEST
                // =========================================================

                if (keys == null || keys.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Please select at least one employee."
                    });
                }


                // =========================================================
                // REMOVE NULL / EMPTY KEYS
                // =========================================================

                keys = keys
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();


                // =========================================================
                // VALIDATE KEYS
                // =========================================================

                if (keys.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "No valid onboarding records were selected."
                    });
                }


                // =========================================================
                // CALL ONBOARDING API
                // =========================================================

                var apiUrl =
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Onboarding,
                        APIApiActionConstants
                            .ApproveEmployeeOnboarding
                    );


                // =========================================================
                // SEND SELECTED PRIVATE KEYS TO API
                // =========================================================

                var response =
                    await _businessLayer.SendPostAPIRequest(
                        keys,
                        apiUrl,
                        "",
                        false
                    );


                // =========================================================
                // READ API RESPONSE
                // =========================================================

                var data =
                    response?.ToString();


                if (string.IsNullOrWhiteSpace(data))
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Unable to approve the selected onboarding records."
                    });
                }


                // =========================================================
                // PARSE API RESPONSE
                // =========================================================

                Newtonsoft.Json.Linq.JObject result;

                try
                {
                    result =
                        Newtonsoft.Json.Linq.JObject.Parse(
                            data);
                }
                catch
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Invalid response received from onboarding API."
                    });
                }


                // =========================================================
                // CHECK API ERROR
                // =========================================================

                string? errorCode =
                    result["errorCode"]?.ToString();


                bool success =
                    string.IsNullOrWhiteSpace(
                        errorCode);


                if (!success)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            result["message"]?.ToString()
                            ?? "Unable to approve the selected onboarding records."
                    });
                }


                // =========================================================
                // GET UPDATED COUNT
                // =========================================================

                int updatedCount =
                    result["updatedCount"] != null &&
                    result["updatedCount"].Type !=
                        Newtonsoft.Json.Linq.JTokenType.Null
                        ? result["updatedCount"]!.Value<int>()
                        : keys.Count;


                // =========================================================
                // SUCCESS
                // =========================================================

                return Json(new
                {
                    success = true,

                    message =
                        $"{updatedCount} employee(s) approved successfully.",

                    updatedCount = updatedCount
                });
            }
            catch (Exception ex)
            {
                // =========================================================
                // EXCEPTION
                // =========================================================

                return Json(new
                {
                    success = false,
                    message =
                        ex.Message
                });
            }
        }

// =========================================================
// REJECT MULTIPLE EMPLOYEE ONBOARDING REQUESTS
// =========================================================

[HttpPost]
public async Task<IActionResult> RejectEmployeeOnboarding(
    [FromBody] List<string> keys)
        {
            try
            {
                // =========================================================
                // CHECK REQUEST
                // =========================================================

                if (keys == null || keys.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Please select at least one employee."
                    });
                }


                // =========================================================
                // REMOVE NULL / EMPTY KEYS
                // =========================================================

                keys = keys
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();


                // =========================================================
                // VALIDATE KEYS
                // =========================================================

                if (keys.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "No valid onboarding records were selected."
                    });
                }


                // =========================================================
                // CALL ONBOARDING API
                // =========================================================

                var apiUrl =
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Onboarding,
                        APIApiActionConstants
                            .RejectEmployeeOnboarding
                    );


                // =========================================================
                // SEND SELECTED PRIVATE KEYS TO API
                // =========================================================

                var response =
                    await _businessLayer.SendPostAPIRequest(
                        keys,
                        apiUrl,
                        "",
                        false
                    );


                // =========================================================
                // READ API RESPONSE
                // =========================================================

                var data =
                    response?.ToString();


                if (string.IsNullOrWhiteSpace(data))
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Unable to reject the selected onboarding records."
                    });
                }


                // =========================================================
                // PARSE API RESPONSE
                // =========================================================

                Newtonsoft.Json.Linq.JObject result;

                try
                {
                    result =
                        Newtonsoft.Json.Linq.JObject.Parse(
                            data);
                }
                catch
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Invalid response received from onboarding API."
                    });
                }


                // =========================================================
                // CHECK API ERROR
                // =========================================================

                string? errorCode =
                    result["errorCode"]?.ToString();


                bool success =
                    string.IsNullOrWhiteSpace(
                        errorCode);


                if (!success)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            result["message"]?.ToString()
                            ?? "Unable to reject the selected onboarding records."
                    });
                }


                // =========================================================
                // GET UPDATED COUNT
                // =========================================================

                int updatedCount =
                    result["updatedCount"] != null &&
                    result["updatedCount"].Type !=
                        Newtonsoft.Json.Linq.JTokenType.Null
                        ? result["updatedCount"]!.Value<int>()
                        : keys.Count;


                // =========================================================
                // SUCCESS
                // =========================================================

                return Json(new
                {
                    success = true,

                    message =
                        $"{updatedCount} employee(s) rejected successfully.",

                    updatedCount = updatedCount
                });
            }
            catch (Exception ex)
            {
                // =========================================================
                // EXCEPTION
                // =========================================================

                return Json(new
                {
                    success = false,
                    message =
                        ex.Message
                });
            }
        }


    }
}
