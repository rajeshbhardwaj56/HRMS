
using DocumentFormat.OpenXml.EMMA;
using HRMS.Models.Common;
using HRMS.Models.EmployeeOnboarding;
using HRMS.Models.LeavePolicy;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace HRMS.Web.Controllers
{
    public class OnboardingController : Controller
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly SalarySlipPdfService _salarySlipPdfService;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IS3Service _s3Service;
        public OnboardingController(IBusinessLayer businessLayer, SalarySlipPdfService salarySlipPdfService,
                        IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider, IS3Service s3Service)
        {
            _businessLayer = businessLayer;
            _salarySlipPdfService = salarySlipPdfService;
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _s3Service = s3Service;
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
                if (model == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid onboarding information."
                    });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                        .Select(x => new
                        {
                            field = x.Key,
                            errors = x.Value!.Errors
                                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                                    ? e.Exception?.Message
                                    : e.ErrorMessage)
                                .ToArray()
                        })
                        .ToArray();

                    return BadRequest(new
                    {
                        success = false,
                        message = "Please correct the validation errors.",
                        errors
                    });
                }

                var apiUrl = _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Onboarding,
                    APIApiActionConstants.StartOnboarding
                );

                var response = await _businessLayer.SendPostAPIRequest(
                    model,
                    apiUrl,
                    "",
                    false
                );

                var data = response?.ToString();

                if (string.IsNullOrWhiteSpace(data))
                {
                    return StatusCode(502, new
                    {
                        success = false,
                        message = "Onboarding API returned an empty response."
                    });
                }

                Debug.WriteLine(
                    "StartOnboarding API Response: " + data
                );

                Result? result;

                try
                {
                    result = JsonConvert.DeserializeObject<Result>(data);
                }
                catch (Exception jsonEx)
                {
                    return StatusCode(502, new
                    {
                        success = false,
                        message = "Invalid response received from onboarding API.",
                        error = jsonEx.Message
                    });
                }

                if (result == null)
                {
                    return StatusCode(502, new
                    {
                        success = false,
                        message = "Invalid response received from onboarding API."
                    });
                }

                if (result.PKNo <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.IsNullOrWhiteSpace(result.Message)
                            ? "Onboarding record was not created."
                            : result.Message
                    });
                }

                long onboardingID = result.PKNo ?? 0;

                string privateKey =
                    _businessLayer.EncodeStringBase64(
                        onboardingID.ToString()
                    );

                string? redirectUrl = Url.Action(
                    "EmploymentForm",
                    "Onboarding",
                    new
                    {
                        key = privateKey
                    }
                );

                return Json(new
                {
                    success = true,
                    onboardingID,
                    privateKey,
                    redirectUrl,
                    message = string.IsNullOrWhiteSpace(result.Message)
                        ? "Onboarding started successfully."
                        : result.Message
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "StartOnboarding ERROR: " + ex
                );

                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    error = ex.Message
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
                // CONVERT STORED FILE KEYS TO FILE URLS
                // =========================================================

                if (!string.IsNullOrWhiteSpace(
                    employeeModel.EmployeePhoto))
                {
                    employeeModel.EmployeePhoto =
                        _s3Service.GetFileUrl(
                            employeeModel.EmployeePhoto);
                }



                if (result.EmployeeOnboardingDocumentsList != null)
                {
                    result.EmployeeOnboardingDocumentsList
                        .ForEach(x =>
                        {
                            if (!string.IsNullOrWhiteSpace(
                                x.FilePath))
                            {
                                x.FilePath =
                                    _s3Service.GetFileUrl(
                                        x.FilePath);
                            }
                        });
                }


                // =========================================================
                // CONVERT WITNESS SIGNATURE FILE PATHS TO FILE URLS
                // =========================================================

                if (result.EmployeeOnboardingWitnessList != null)
                {
                    result.EmployeeOnboardingWitnessList
                        .ForEach(x =>
                        {
                            if (!string.IsNullOrWhiteSpace(
                                x.SignatureFilePath))
                            {
                                x.SignatureFilePath =
                                    _s3Service.GetFileUrl(
                                        x.SignatureFilePath);
                            }
                        });
                }
                // =========================================================
                // GET LAST SAVED STEP
                // =========================================================

                int lastSavedStep =
                    employeeModel.LastSavedStep;


                if (lastSavedStep < 1)
                {
                    lastSavedStep = 1;
                }

                if (lastSavedStep > 9)
                {
                    lastSavedStep = 9;
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
                        // =====================================================
                        // WITNESS
                        // =====================================================

                        WitnessList =
                    result.EmployeeOnboardingWitnessList
                    ?? new List<EmployeeOnboardingWitnessModel>(),
                        // =====================================================
                        // REPORTING MANAGERS
                        // =====================================================

                        ReportingManagerList =
    result.EmployeeOnboardingReportingManagerList
    ?? new List<EmployeeOnboardingReportingManagerModel>(),
                        OnboardingJobLocationList =
        result.OnboardingJobLocationList
        ?? new List<SelectListItem>(),

                        OnboardingEmployeeTypeList =
        result.OnboardingEmployeeTypeList
        ?? new List<SelectListItem>(),

                        OnboardingPayrollTypeList =
        result.OnboardingPayrollTypeList
        ?? new List<SelectListItem>(),

                        OnboardingDepartmentList =
        result.OnboardingDepartmentList
        ?? new List<SelectListItem>(),

                        OnboardingSubDepartmentList =
        result.OnboardingSubDepartmentList
        ?? new List<SelectListItem>(),

                        OnboardingDesignationList =
        result.OnboardingDesignationList
        ?? new List<SelectListItem>(),

                        OnboardingShiftTypeList =
        result.OnboardingShiftTypeList
        ?? new List<SelectListItem>(),

                        OnboardingReportingManagerList =
        result.OnboardingReportingManagerList
        ?? new List<SelectListItem>(),

                        OnboardingLeavePolicyList =
        result.OnboardingLeavePolicyList
        ?? new List<LeavePolicyModel>(),

                        OnboardingRoleList =
        result.OnboardingRoleList
        ?? new List<SelectListItem>(),
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
                    model.CurrentStep > 9)
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
                model.WitnessList ??=
                     new List<EmployeeOnboardingWitnessModel>();

                model.ReportingManagerList ??=
                    new List<EmployeeOnboardingReportingManagerModel>();

                // =========================================================
                // ONBOARDING FILE STORAGE FOLDER
                // =========================================================

                string onboardingFolder =
                    $"employee-onboarding/{model.OnboardingID}";


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

                    // =========================================================
                    // UPLOAD TO FILE STORAGE
                    // =========================================================

                    string uploadedKey =
                        _s3Service.UploadFileToFolder(
                            model.EmployeePhotoFile,
                            onboardingFolder,
                            fileName
                        );

                    // =========================================================
                    // STORE ONLY KEY IN DATABASE
                    // =========================================================

                    model.EmployeePhoto =
                        uploadedKey;
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
                        Path.GetExtension(file.FileName)
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
                        safeDocumentType = "Document";
                    }

                    // -----------------------------------------------------
                    // UNIQUE FILE NAME
                    // -----------------------------------------------------

                    string fileName =
                        $"{safeDocumentType}_" +
                        $"{Guid.NewGuid():N}" +
                        $"{extension}";

                    // =====================================================
                    // UPLOAD TO ONBOARDING FOLDER
                    // =====================================================

                    string uploadedKey =
                        _s3Service.UploadFileToFolder(
                            file,
                            onboardingFolder,
                            fileName
                        );

                    if (string.IsNullOrWhiteSpace(uploadedKey))
                    {
                        throw new Exception(
                            $"Unable to upload {documentType}."
                        );
                    }

                    // =====================================================
                    // ADD DOCUMENT TO DOCUMENT LIST
                    // =====================================================

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

                            // Store storage key
                            FilePath =
                                uploadedKey,

                            UploadedAt =
                                DateTime.Now,

                            DocumentReferenceID =
                                documentReferenceID
                        });

                    return uploadedKey;
                }
                // =========================================================
                // HELPER - SAVE BASE64 SIGNATURE
                // =========================================================

                async Task<string?> SaveBase64Signature(
                    string? base64,
                    string documentType)
                {
                    if (string.IsNullOrWhiteSpace(base64))
                    {
                        return null;
                    }

                    // -----------------------------------------------------
                    // REMOVE DATA URL PREFIX
                    // Example:
                    // data:image/png;base64,xxxxx
                    // -----------------------------------------------------

                    string cleanBase64 = base64;

                    if (base64.Contains(","))
                    {
                        cleanBase64 =
                            base64.Substring(
                                base64.IndexOf(",") + 1
                            );
                    }

                    byte[] fileBytes;

                    try
                    {
                        fileBytes =
                            Convert.FromBase64String(
                                cleanBase64);
                    }
                    catch
                    {
                        throw new Exception(
                            $"Invalid signature data for {documentType}."
                        );
                    }

                    // -----------------------------------------------------
                    // MAX 5 MB
                    // -----------------------------------------------------

                    if (fileBytes.Length >
                        5 * 1024 * 1024)
                    {
                        throw new Exception(
                            $"{documentType} signature cannot exceed 5 MB."
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
                            "Signature";
                    }

                    // -----------------------------------------------------
                    // UNIQUE FILE NAME
                    // -----------------------------------------------------

                    string fileName =
                        $"{safeDocumentType}_" +
                        $"{Guid.NewGuid():N}.png";

                    // =====================================================
                    // CONVERT BYTE[] TO IFORMFILE
                    // =====================================================

                    await using var inputStream =
                        new MemoryStream(fileBytes);

                    var formFile =
                        new FormFile(
                            inputStream,
                            0,
                            fileBytes.Length,
                            documentType,
                            fileName)
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = "image/png"
                        };

                    // =====================================================
                    // UPLOAD USING EXISTING FILE SERVICE
                    // =====================================================

                    string uploadedKey =
                        _s3Service.UploadFileToFolder(
                            formFile,
                            onboardingFolder,
                            fileName
                        );

                    if (string.IsNullOrWhiteSpace(
                        uploadedKey))
                    {
                        throw new Exception(
                            $"Unable to upload {documentType} signature."
                        );
                    }

                    // =====================================================
                    // ADD DOCUMENT TO DOCUMENT LIST
                    // =====================================================

                    model.DocumentsList.Add(
                        new EmployeeOnboardingDocumentsModel
                        {
                            DocumentID = 0,

                            OnboardingID =
                                model.OnboardingID,

                            DocumentType =
                                documentType,

                            FileName =
                                fileName,

                            FilePath =
                                uploadedKey,

                            UploadedAt =
                                DateTime.Now
                        });

                    return uploadedKey;
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
                // STEP 7 - INTERVIEW / APPROVAL SIGNATURES
                // =========================================================

                if (model.CurrentStep == 8)
                {
                    // =====================================================
                    // HR INTERVIEWER SIGNATURE
                    // =====================================================

                    string? hrInterviewerSignaturePath = null;

                    if (!string.IsNullOrWhiteSpace(
                        model.HRInterviewerSignatureBase64))
                    {
                        hrInterviewerSignaturePath =
                            await SaveBase64Signature(
                                model.HRInterviewerSignatureBase64,
                                "HRInterviewerSignature"
                            );
                    }
                    else
                    {
                        hrInterviewerSignaturePath =
                            await SaveDocument(
                                model.HRInterviewerSignatureFile,
                                "HRInterviewerSignature"
                            );
                    }


                    // =====================================================
                    // OPERATIONS INTERVIEWER SIGNATURE
                    // =====================================================

                    string? operationsInterviewerSignaturePath = null;

                    if (!string.IsNullOrWhiteSpace(
                        model.OperationsInterviewerSignatureBase64))
                    {
                        operationsInterviewerSignaturePath =
                            await SaveBase64Signature(
                                model.OperationsInterviewerSignatureBase64,
                                "OperationsInterviewerSignature"
                            );
                    }
                    else
                    {
                        operationsInterviewerSignaturePath =
                            await SaveDocument(
                                model.OperationsInterviewerSignatureFile,
                                "OperationsInterviewerSignature"
                            );
                    }


                    // =====================================================
                    // PROCESS OWNER INTERVIEWER SIGNATURE
                    // =====================================================

                    string? processOwnerInterviewerSignaturePath = null;

                    if (!string.IsNullOrWhiteSpace(
                        model.ProcessOwnerInterviewerSignatureBase64))
                    {
                        processOwnerInterviewerSignaturePath =
                            await SaveBase64Signature(
                                model.ProcessOwnerInterviewerSignatureBase64,
                                "ProcessOwnerInterviewerSignature"
                            );
                    }
                    else
                    {
                        processOwnerInterviewerSignaturePath =
                            await SaveDocument(
                                model.ProcessOwnerInterviewerSignatureFile,
                                "ProcessOwnerInterviewerSignature"
                            );
                    }


                    // =====================================================
                    // MAIN INTERVIEWER SIGNATURE
                    // =====================================================

                    string? interviewerSignaturePath = null;

                    if (!string.IsNullOrWhiteSpace(
                        model.InterviewerSignatureBase64))
                    {
                        interviewerSignaturePath =
                            await SaveBase64Signature(
                                model.InterviewerSignatureBase64,
                                "InterviewerSignature"
                            );
                    }
                    else
                    {
                        interviewerSignaturePath =
                            await SaveDocument(
                                model.InterviewerSignatureFile,
                                "InterviewerSignature"
                            );
                    }


                    // =====================================================
                    // CANDIDATE / EMPLOYEE SIGNATURE
                    // =====================================================

                    string? signaturePath = null;

                    if (!string.IsNullOrWhiteSpace(
                        model.SignatureBase64))
                    {
                        signaturePath =
                            await SaveBase64Signature(
                                model.SignatureBase64,
                                "Signature"
                            );
                    }
                    else
                    {
                        signaturePath =
                            await SaveDocument(
                                model.SignatureFile,
                                "Signature"
                            );
                    }


                    // =====================================================
                    // FUNCTIONAL HEAD APPROVAL
                    // =====================================================

                    string? functionalHeadApprovalPath = null;

                    if (!string.IsNullOrWhiteSpace(
                        model.FunctionalHeadApprovalBase64))
                    {
                        functionalHeadApprovalPath =
                            await SaveBase64Signature(
                                model.FunctionalHeadApprovalBase64,
                                "FunctionalHeadApproval"
                            );
                    }
                    else
                    {
                        functionalHeadApprovalPath =
                            await SaveDocument(
                                model.FunctionalHeadApprovalFile,
                                "FunctionalHeadApproval"
                            );
                    }


                    // =====================================================
                    // SPOC HUMAN RESOURCES APPROVAL
                    // =====================================================

                    string? spocHRApprovalPath = null;

                    if (!string.IsNullOrWhiteSpace(
                        model.SPOCHumanResourcesApprovalBase64))
                    {
                        spocHRApprovalPath =
                            await SaveBase64Signature(
                                model.SPOCHumanResourcesApprovalBase64,
                                "SPOCHumanResourcesApproval"
                            );
                    }
                    else
                    {
                        spocHRApprovalPath =
                            await SaveDocument(
                                model.SPOCHumanResourcesApprovalFile,
                                "SPOCHumanResourcesApproval"
                            );
                    }


                    // =====================================================
                    // HEAD HUMAN RESOURCES APPROVAL
                    // =====================================================

                    string? headHRApprovalPath = null;

                    if (!string.IsNullOrWhiteSpace(
                        model.HeadHumanResourcesApprovalBase64))
                    {
                        headHRApprovalPath =
                            await SaveBase64Signature(
                                model.HeadHumanResourcesApprovalBase64,
                                "HeadHumanResourcesApproval"
                            );
                    }
                    else
                    {
                        headHRApprovalPath =
                            await SaveDocument(
                                model.HeadHumanResourcesApprovalFile,
                                "HeadHumanResourcesApproval"
                            );
                    }

                    // =====================================================
                    // MAP STEP 8 SIGNATURE PATHS
                    // =====================================================

                    if (model.InterviewEvaluationList == null)
                    {
                        model.InterviewEvaluationList =
                            new List<EmployeeOnboardingInterviewEvaluationModel>();
                    }

                    if (model.InterviewEvaluationList.Count == 0)
                    {
                        model.InterviewEvaluationList.Add(
                            new EmployeeOnboardingInterviewEvaluationModel
                            {
                                InterviewEvaluationID = 0,
                                OnboardingID = model.OnboardingID
                            });
                    }

                    var interviewEvaluation =
                        model.InterviewEvaluationList[0];

                    interviewEvaluation.OnboardingID =
                        model.OnboardingID;


                    // =====================================================
                    // MAIN INTERVIEWER
                    // =====================================================

                    if (!string.IsNullOrWhiteSpace(interviewerSignaturePath))
                    {
                        interviewEvaluation.InterviewerSignature =
                            interviewerSignaturePath;
                    }


                    // =====================================================
                    // EMPLOYEE / CANDIDATE SIGNATURE
                    // =====================================================

                    if (!string.IsNullOrWhiteSpace(signaturePath))
                    {
                        interviewEvaluation.Signature =
                            signaturePath;
                    }


                    // =====================================================
                    // FUNCTIONAL HEAD
                    // =====================================================

                    if (!string.IsNullOrWhiteSpace(functionalHeadApprovalPath))
                    {
                        interviewEvaluation.FunctionalHeadApproval =
                            functionalHeadApprovalPath;
                    }


                    // =====================================================
                    // SPOC HUMAN RESOURCES
                    // =====================================================

                    if (!string.IsNullOrWhiteSpace(spocHRApprovalPath))
                    {
                        interviewEvaluation.SPOCHumanResourcesApproval =
                            spocHRApprovalPath;
                    }


                    // =====================================================
                    // HEAD HUMAN RESOURCES
                    // =====================================================

                    if (!string.IsNullOrWhiteSpace(headHRApprovalPath))
                    {
                        interviewEvaluation.HeadHumanResourcesApproval =
                            headHRApprovalPath;
                    }

                    // =========================================================
                    // OFFER LETTER
                    // =========================================================

                    if (model.OfferLetterFile != null &&
                        model.OfferLetterFile.Length > 0)
                    {
                        string? offerLetterPath = await SaveDocument(
                            model.OfferLetterFile,
                            "OfferLetter"
                        );

                        model.OfferLetterDocumentPath = offerLetterPath;
                    }
                    // =====================================================
                    // WITNESS SIGNATURES
                    // =====================================================
                    // =====================================================
                    // WITNESS SIGNATURES
                    // =====================================================

                    if (model.WitnessList != null &&
                        model.WitnessList.Count > 0)
                    {
                        foreach (var witness in model.WitnessList)
                        {
                            if (witness == null)
                                continue;

                            string? witnessSignaturePath = null;

                            // -------------------------------------------------
                            // DRAW / TYPE SIGNATURE
                            // -------------------------------------------------

                            if (!string.IsNullOrWhiteSpace(
                                witness.SignatureBase64))
                            {
                                witnessSignaturePath =
                                    await SaveBase64Signature(
                                        witness.SignatureBase64,
                                        "WitnessSignature"
                                    );
                            }

                            // -------------------------------------------------
                            // UPLOAD SIGNATURE
                            // -------------------------------------------------

                            else if (witness.SignatureFile != null &&
                                     witness.SignatureFile.Length > 0)
                            {
                                witnessSignaturePath =
                                    await SaveDocument(
                                        witness.SignatureFile,
                                        "WitnessSignature"
                                    );
                            }

                            // -------------------------------------------------
                            // SAVE PATH INTO WITNESS MODEL
                            // -------------------------------------------------

                            if (!string.IsNullOrWhiteSpace(
                                witnessSignaturePath))
                            {
                                witness.SignatureFilePath =
                                    witnessSignaturePath;

                                witness.SignatureFileName =
                                    Path.GetFileName(
                                        witnessSignaturePath);
                            }

                            // -------------------------------------------------
                            // NEVER SEND FILE/BASE64 TO API
                            // -------------------------------------------------

                            witness.SignatureFile = null;
                            witness.SignatureBase64 = null;
                        }
                    }
                }

                // =========================================================
                // STEP 7 - IDENTITY + EMPLOYEE DOCUMENTS
                // =========================================================

                // Save these documents whenever files are actually supplied.
                // This avoids losing them if CurrentStep is changed by the UI.

                // =========================================================
                // PASSPORT
                // =========================================================

if (model.PassportFile != null &&
    model.PassportFile.Length > 0)
                {
                    string? passportPath =
                        await SaveDocument(
                            model.PassportFile,
                            "Passport"
                        );

                    model.PassportDocumentPath = passportPath;
                }


                // =========================================================
                // PAN CARD
                // =========================================================

                if (model.PANFile != null &&
                    model.PANFile.Length > 0)
                {
                    string? panPath =
                        await SaveDocument(
                            model.PANFile,
                            "PAN"
                        );

                    model.PANDocumentPath = panPath;
                }


                // =========================================================
                // AADHAAR CARD
                // =========================================================

                if (model.AadhaarFile != null &&
                    model.AadhaarFile.Length > 0)
                {
                    string? aadhaarPath =
                        await SaveDocument(
                            model.AadhaarFile,
                            "Aadhaar"
                        );

                    model.AadhaarDocumentPath = aadhaarPath;
                }


                // =========================================================
                // EMPLOYEE SIGNATURE
                // =========================================================

                string? employeeSignaturePath = null;

                if (!string.IsNullOrWhiteSpace(
                        model.EmployeeSignatureBase64))
                {
                    employeeSignaturePath =
                        await SaveBase64Signature(
                            model.EmployeeSignatureBase64,
                            "EmployeeSignature"
                        );
                }
                else if (model.EmployeeSignatureFile != null &&
                         model.EmployeeSignatureFile.Length > 0)
                {
                    employeeSignaturePath =
                        await SaveDocument(
                            model.EmployeeSignatureFile,
                            "EmployeeSignature"
                        );
                }

                if (!string.IsNullOrWhiteSpace(employeeSignaturePath))
                {
                    model.EmployeeSignaturePath =
                        employeeSignaturePath;

                    model.EmployeeSignature =
                        employeeSignaturePath;
                }


                // =========================================================
                // EXPERIENCE CERTIFICATE
                // =========================================================

                if (model.ExperienceCertificateFile != null &&
                    model.ExperienceCertificateFile.Length > 0)
                {
                    string? experienceCertificatePath =
                        await SaveDocument(
                            model.ExperienceCertificateFile,
                            "ExperienceCertificate"
                        );

                    model.ExperienceCertificateDocumentPath =
                        experienceCertificatePath;
                }


                // =========================================================
                // SALARY SLIP
                // =========================================================

                if (model.SalarySlipFile != null &&
                    model.SalarySlipFile.Length > 0)
                {
                    string? salarySlipPath =
                        await SaveDocument(
                            model.SalarySlipFile,
                            "SalarySlip"
                        );

                    model.SalarySlipDocumentPath =
                        salarySlipPath;
                }


                // =========================================================
                // RESUME
                // =========================================================

                if (model.ResumeFile != null &&
                    model.ResumeFile.Length > 0)
                {
                    string? resumePath =
                        await SaveDocument(
                            model.ResumeFile,
                            "Resume"
                        );

                    model.ResumeDocumentPath =
                        resumePath;
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
                model.OfferLetterFile = null;

                model.ExperienceCertificateFile = null;

                model.SalarySlipFile = null;

                model.ResumeFile = null;
                if (model.WitnessList != null)
                {
                    foreach (var witness in model.WitnessList)
                    {
                        witness.SignatureFile = null;
                    }
                }
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
                // =========================================================
                // STEP 8 DEBUG
                // =========================================================

                System.Diagnostics.Debug.WriteLine(
                    "====================================================");

                System.Diagnostics.Debug.WriteLine(
                    $"STEP 8 OnboardingID = {model.OnboardingID}");

                System.Diagnostics.Debug.WriteLine(
                    $"InterviewEvaluationList Count = " +
                    $"{model.InterviewEvaluationList?.Count ?? 0}");

                if (model.InterviewEvaluationList != null &&
                    model.InterviewEvaluationList.Count > 0)
                {
                    var evaluation =
                        model.InterviewEvaluationList[0];

                    System.Diagnostics.Debug.WriteLine(
                        $"FunctionalHeadApproval = " +
                        $"{evaluation.FunctionalHeadApproval}");

                    System.Diagnostics.Debug.WriteLine(
                        $"SPOCHumanResourcesApproval = " +
                        $"{evaluation.SPOCHumanResourcesApproval}");

                    System.Diagnostics.Debug.WriteLine(
                        $"HeadHumanResourcesApproval = " +
                        $"{evaluation.HeadHumanResourcesApproval}");
                }

                System.Diagnostics.Debug.WriteLine(
                    "====================================================");


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


                if (lastSavedStep > 9)
                {
                    lastSavedStep = 9;
                }


                // =========================================================
                // NEXT STEP
                // =========================================================

                int nextStep =
                    currentStep < 9
                        ? currentStep + 1
                        : 9;


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
                        currentStep == 9,

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
                // CONVERT STORED FILE KEYS TO FILE URLS
                // =========================================================

                if (!string.IsNullOrWhiteSpace(
                    employeeModel.EmployeePhoto))
                {
                    employeeModel.EmployeePhoto =
                        _s3Service.GetFileUrl(
                            employeeModel.EmployeePhoto);
                }

                if (result.EmployeeOnboardingDocumentsList != null)
                {
                    result.EmployeeOnboardingDocumentsList
                        .ForEach(x =>
                        {
                            if (!string.IsNullOrWhiteSpace(
                                x.FilePath))
                            {
                                x.FilePath =
                                    _s3Service.GetFileUrl(
                                        x.FilePath);
                            }
                        });
                }
                // =========================================================
                // CONVERT WITNESS SIGNATURE FILE PATHS TO FILE URLS
                // =========================================================

                if (result.EmployeeOnboardingWitnessList != null)
                {
                    result.EmployeeOnboardingWitnessList
                        .ForEach(x =>
                        {
                            if (!string.IsNullOrWhiteSpace(
                                x.SignatureFilePath))
                            {
                                x.SignatureFilePath =
                                    _s3Service.GetFileUrl(
                                        x.SignatureFilePath);
                            }
                        });
                }
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
                        // WITNESS
                        // =====================================================

                        WitnessList =
                    result.EmployeeOnboardingWitnessList
                    ?? new List<EmployeeOnboardingWitnessModel>(),
                        // =====================================================
                        // COMMON
                        // =====================================================
                        // =====================================================
                        // REPORTING MANAGERS
                        // =====================================================

                        ReportingManagerList =
    result.EmployeeOnboardingReportingManagerList
    ?? new List<EmployeeOnboardingReportingManagerModel>(),
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


[AllowAnonymous]
[HttpGet]
public async Task<IActionResult> Print(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return RedirectToAction("Index", "Onboarding");
            }

            try
            {
                // =========================================================
                // GET COMPLETE EMPLOYMENT FORM MODEL
                // =========================================================

                var model = GetEmploymentFormModel(key);

                if (model == null)
                {
                    return NotFound();
                }

                // =========================================================
                // SET VIEW MODE
                // =========================================================

                ViewBag.IsViewMode = true;
                ViewBag.OnboardingID = model.OnboardingID;
                ViewBag.Key = key;

                // =========================================================
                // RENDER COMPLETE VIEW TO HTML
                // =========================================================

                var html = await RenderViewToStringAsync(
                    "ViewEmploymentForm",
                    model
                );

                // =========================================================
                // GENERATE PDF USING PLAYWRIGHT
                // =========================================================

                var pdfBytes =
                    await _salarySlipPdfService.GeneratePdfAsync(html);

                // =========================================================
                // RETURN PDF
                // =========================================================

                return File(
                    pdfBytes,
                    "application/pdf"
                );
            }
            catch (Exception ex)
            {
                // Add your existing logger here if you have one.
                return StatusCode(
                    500,
                    $"Unable to generate employment form PDF. {ex.Message}"
                );
            }
        }


private EmployeeOnboardingPageViewModel GetEmploymentFormModel(string key)
        {
            // =========================================================
            // DECODE KEY
            // =========================================================

            string decodedValue =
                _businessLayer.DecodeStringBase64(key);

            if (!long.TryParse(decodedValue, out long onboardingID))
            {
                return null;
            }

            if (onboardingID <= 0)
            {
                return null;
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
                return null;
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
                return null;
            }

            if (result == null)
            {
                return null;
            }

            // =========================================================
            // EMPLOYEE DETAILS
            // =========================================================

            EmployeeDetailsOnboardingModel employeeModel =
                result.EmployeeDetailsOnboardingList?
                    .FirstOrDefault()
                ?? new EmployeeDetailsOnboardingModel();

            employeeModel.OnboardingID = onboardingID;

            // =========================================================
            // CREATE COMPLETE VIEW MODEL
            // =========================================================

            var model =
                new EmployeeOnboardingPageViewModel
                {
                    // -------------------------------------------------
                    // Employee
                    // -------------------------------------------------

                    EmployeeDetails =
                        employeeModel,

                    // -------------------------------------------------
                    // Family
                    // -------------------------------------------------

                    FamilyList =
                        result.EmployeeOnboardingFamilyList
                        ?? new List<EmployeeOnboardingFamilyModel>(),

                    // -------------------------------------------------
                    // Education
                    // -------------------------------------------------

                    EducationList =
                        result.EmployeeOnboardingEducationList
                        ?? new List<EmployeeOnboardingEducationModel>(),

                    // -------------------------------------------------
                    // Employment
                    // -------------------------------------------------

                    EmploymentList =
                        result.EmployeeOnboardingEmploymentList
                        ?? new List<EmployeeOnboardingEmploymentModel>(),

                    // -------------------------------------------------
                    // Bank
                    // -------------------------------------------------

                    BankDetailsList =
                        result.EmployeeOnboardingBankDetailsList
                        ?? new List<EmployeeOnboardingBankDetailsModel>(),

                    // -------------------------------------------------
                    // Documents
                    // -------------------------------------------------

                    DocumnetList =
                        result.EmployeeOnboardingDocumentsList
                        ?? new List<EmployeeOnboardingDocumentsModel>(),

                    // -------------------------------------------------
                    // EPF Nominees
                    // -------------------------------------------------

                    EPFNomineeList =
                        result.EmployeeOnboardingNomineeList?
                            .Where(x => x.NomineeType == "EPF")
                            .ToList()
                        ?? new List<EmployeeOnboardingNomineeModel>(),

                    // -------------------------------------------------
                    // EPS Nominees
                    // -------------------------------------------------

                    EPSNomineeList =
                        result.EmployeeOnboardingNomineeList?
                            .Where(x => x.NomineeType == "EPS")
                            .ToList()
                        ?? new List<EmployeeOnboardingNomineeModel>(),

                    // -------------------------------------------------
                    // Gratuity Nominees
                    // -------------------------------------------------

                    GratuityNomineeList =
                        result.EmployeeOnboardingNomineeList?
                            .Where(x => x.NomineeType == "Gratuity")
                            .ToList()
                        ?? new List<EmployeeOnboardingNomineeModel>(),

                    // -------------------------------------------------
                    // Interview Evaluation
                    // -------------------------------------------------

                    InterviewEvaluationList =
                        result.EmployeeOnboardingInterviewEvaluationList
                        ?? new List<EmployeeOnboardingInterviewEvaluationModel>(),

                    // -------------------------------------------------
                    // Common
                    // -------------------------------------------------

                    OnboardingID = onboardingID,

                    Key = key,

                    LastSavedStep = 8,

                    CurrentStep = 1
                };

            return model;
        }


        private async Task<string> RenderViewToStringAsync(
   string viewName,
   object model)
        {
            var actionContext = new ActionContext(
                HttpContext,
                RouteData,
                ControllerContext.ActionDescriptor,
                ModelState
            );

            var viewEngineResult = _razorViewEngine.FindView(
                actionContext,
                viewName,
                false
            );

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException(
                    $"View '{viewName}' could not be found."
                );
            }

            var view = viewEngineResult.View;

            await using var sw = new StringWriter();

            var viewData = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            var tempData = new TempDataDictionary(
                HttpContext,
                _tempDataProvider
            );

            var viewContext = new ViewContext(
                actionContext,
                view,
                viewData,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await view.RenderAsync(viewContext);

            return sw.ToString();
        }


[HttpPost]
public async Task<IActionResult> CompleteEmployeeOnboarding(
    [FromBody] CompleteEmployeeOnboardingRequest request)
        {
            try
            {
                // =========================================================
                // CHECK REQUEST
                // =========================================================

                if (request == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid onboarding request."
                    });
                }


                // =========================================================
                // VALIDATE ONBOARDING ID
                // =========================================================

                if (request.OnboardingID <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid onboarding ID."
                    });
                }


                // =========================================================
                // GET LOGGED-IN USER
                // =========================================================

                long approvedBy = 0;

                var userID =
                    HttpContext.Session.GetString("UserID");

                if (!string.IsNullOrWhiteSpace(userID))
                {
                    long.TryParse(
                        userID,
                        out approvedBy
                    );
                }


                if (approvedBy <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Unable to identify the logged-in user."
                    });
                }


                // =========================================================
                // CREATE API REQUEST
                // =========================================================

                var apiRequest = new
                {
                    OnboardingID = request.OnboardingID,

                    ApprovedBy = approvedBy,

                    ApprovalComments =
                        string.IsNullOrWhiteSpace(
                            request.ApprovalComments)
                            ? null
                            : request.ApprovalComments.Trim()
                };


                // =========================================================
                // GET ONBOARDING API URL
                // =========================================================

                var apiUrl =
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Onboarding,
                        APIApiActionConstants
                            .CompleteEmployeeOnboarding
                    );


                // =========================================================
                // CALL ONBOARDING API
                // =========================================================

                var response =
                    await _businessLayer.SendPostAPIRequest(
                        apiRequest,
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
                            "Unable to complete employee onboarding."
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
                            ?? "Unable to complete employee onboarding."
                    });
                }


                // =========================================================
                // GET EMPLOYEE ID
                // =========================================================

                long employeeID = 0;

                if (result["employeeID"] != null &&
                    result["employeeID"].Type !=
                        Newtonsoft.Json.Linq.JTokenType.Null)
                {
                    employeeID =
                        result["employeeID"]!.Value<long>();
                }


                // =========================================================
                // SUCCESS
                // =========================================================

                return Json(new
                {
                    success = true,

                    message =
                        result["message"]?.ToString()
                        ?? "Employee onboarding completed successfully.",

                    employeeID = employeeID
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




    }

}
