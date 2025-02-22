﻿using Common;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using ViewLayer.Models;

namespace ViewLayer.Controllers
{
    public class HomeController : Controller
    {
        private ItemController itemController = new ItemController();
        private StudentController studentController = new StudentController();
        private TestController testController = new TestController();

        public string GetUserLoginEmail()
        {
            if (Settings.TestingUser.role > -2) { return Settings.TestingUser.loginEmail; }

            return (User != null ? ((ClaimsIdentity)User.Identity).Claims.ToList()[2].Value : "");
        }

        public int GetUserRole()
        {
            if (Settings.TestingUser.role > -2) { return Settings.TestingUser.role; }

            try { return studentController.LoadStudentByEmail(GetUserLoginEmail()).role; }
            catch { return -1; }
        }

        public bool CanUserAccessPage(string checkStudentIdentifier, int minRequiredRole, int maxRequiredRole)
        {
            int userRole = GetUserRole();
            if (userRole >= minRequiredRole && userRole <= maxRequiredRole)
            {
                if (userRole == 0)
                {
                    string userStudentIdentifier = "";
                    try { userStudentIdentifier = studentController.LoadStudentByEmail(GetUserLoginEmail()).studentIdentifier; }
                    catch { }

                    if(checkStudentIdentifier != "" && userStudentIdentifier != checkStudentIdentifier)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool CanUserAccessPage(string checkStudentIdentifier, int minRequiredRole)
        {
            int userRole = GetUserRole();
            if (userRole >= minRequiredRole)
            {
                if (userRole == 0)
                {
                    string userStudentIdentifier = "";
                    try { userStudentIdentifier = studentController.LoadStudentByEmail(GetUserLoginEmail()).studentIdentifier; }
                    catch { }

                    if (checkStudentIdentifier != "" && userStudentIdentifier != checkStudentIdentifier)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public (string message, string messageClass) GetHeaderMessageData()
        {
            (string message, string messageClass) headerMessageData = ("", "");

            // Testing header message
            if (Settings.Testing)
            {
                headerMessageData.message = "Aktuálně je nastaven režim pro testování.";
                headerMessageData.messageClass = "info";
            }

            return headerMessageData;
        }

        [HttpGet]
        public IActionResult Index(string error = "")
        {
            // Check if user can access page
            if (!CanUserAccessPage("", -1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)> students = studentController.LoadStudentsByEmail();
            List<(string roleText, List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)> students)> studentsByRoles = new List<(string roleText, List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)> students)>();
            string[] rolesTexts = new string[] { "Studenti", "Učitelé", "Správci" };
            for (int i = 0; i < 3; i++)
            {
                studentsByRoles.Add((rolesTexts[i], new List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)>()));
            }
            foreach ((string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email) student in students)
            {
                studentsByRoles[student.role].students.Add(student);
            }

            string text = "", textClass = "";

            switch (error)
            {
                case "access_denied":
                    text = "Přístup odepřen.";
                    textClass = "incorrect";
                    break;
                case "unexpected_exception":
                    text = "Při přihlášení nastala neočekávaná chyba.\n" +
                        "Zkuste se prosím znovu přihlásit později.";
                    textClass = "incorrect";
                    break;
                case "user_not_found_exception":
                    text = "Uživatel s tímto emailem v systému neexistuje.\n" +
                        "Požádejte prosím správce systému, aby tento email spároval s vaším uživatelským účtem.";
                    textClass = "partiallyCorrect";
                    break;
                case "testing_disabled_exception":
                    text = "Přihlášení pomocí testovacího uživatele nebylo možné dokončit.\n" +
                        "Pro přihlášení pomocí testovacího uživatele je nejprve potřeba nastavit režim pro testování.";
                    textClass = "incorrect";
                    break;
                default:
                    if (students.Count == 0)
                    {
                        text = "Systém dosud neobsahuje žádného uživatele.\n" +
                            "Po úspěšném přihlášení bude váš účet nastaven do role správce.";
                        textClass = "info";
                    }
                    break;
            }

            return View(new IndexModel
            {
                Title = "Přihlášení",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                StudentsByRoles = studentsByRoles,
                Text = text,
                TextClass = textClass,
                SignInURL = Settings.GetSignInURL()
            });
        }

        [HttpGet]
        public IActionResult ManageUserList(string text = "")
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 2, 2)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)> students = studentController.LoadStudentsByEmail();
            List<(string roleText, List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)> students)> studentsByRoles = new List<(string roleText, List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)> students)>();
            string[] rolesTexts = new string[] { "Studenti", "Učitelé", "Správci" };
            for (int i = 0; i < 3; i++)
            {
                studentsByRoles.Add((rolesTexts[i], new List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)>()));
            }
            foreach ((string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email) student in students)
            {
                studentsByRoles[student.role].students.Add(student);
            }

            List<(string studentNumberIdentifier, string studentIdentifier, string login, string firstName, string lastName, string email)> studentsOfTao = studentController.LoadStudents();
            List<(string studentNumberIdentifier, string studentIdentifier, string login, string firstName, string lastName, string email)> studentsOfTaoPaired = new List<(string studentNumberIdentifier, string studentIdentifier, string login, string firstName, string lastName, string email)>();
            foreach ((string studentNumberIdentifier, string studentIdentifier, string login, string firstName, string lastName, string email) studentOfTao in studentsOfTao)
            {
                foreach ((string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email) student in students)
                {
                    if (studentOfTao.studentNumberIdentifier == student.studentNumberIdentifier)
                    {
                        studentsOfTaoPaired.Add(studentOfTao);
                        break;
                    }
                }
            }

            ManageUserListModel model = new ManageUserListModel
            {
                Title = "Správa uživatelů",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                Students = students,
                StudentsByRoles = studentsByRoles,
                StudentsOfTao = studentsOfTao,
                StudentsOfTaoNotPaired = studentsOfTao.Except(studentsOfTaoPaired).ToList(),
                RoleTexts = new string[] { "Student", "Učitel", "Správce" },
                LoginEmail = "",
                Role = "",
                StudentNumberIdentifier = ""
            };
            switch (text)
            {
                case "user_successfully_added":
                    model.Text = "Uživatel byl úspěšně přidán.";
                    model.TextClass = "correct";
                    break;
                case "user_successfully_deleted":
                    model.Text = "Uživatel byl úspěšně odebrán.";
                    model.TextClass = "correct";
                    break;
                case "last_admin_cannot_be_deleted":
                    model.Text = "Poslední správce nemůže být odebrán.";
                    model.TextClass = "incorrect";
                    break;
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult ManageUserList(string loginEmail, string role, string studentNumberIdentifier = "")
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 2, 2)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)> students = studentController.LoadStudentsByEmail();
            List<(string roleText, List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)> students)> studentsByRoles = new List<(string roleText, List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)> students)>();
            string[] rolesTexts = new string[] { "Studenti", "Učitelé", "Správci" };
            for (int i = 0; i < 3; i++)
            {
                studentsByRoles.Add((rolesTexts[i], new List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)>()));
            }
            foreach ((string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email) student in students)
            {
                studentsByRoles[student.role].students.Add(student);
            }

            List<(string studentNumberIdentifier, string studentIdentifier, string login, string firstName, string lastName, string email)> studentsOfTao = studentController.LoadStudents();
            List<(string studentNumberIdentifier, string studentIdentifier, string login, string firstName, string lastName, string email)> studentsOfTaoPaired = new List<(string studentNumberIdentifier, string studentIdentifier, string login, string firstName, string lastName, string email)>();
            foreach ((string studentNumberIdentifier, string studentIdentifier, string login, string firstName, string lastName, string email) studentOfTao in studentsOfTao)
            {
                foreach ((string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email) student in students)
                {
                    if (studentOfTao.studentNumberIdentifier == student.studentNumberIdentifier)
                    {
                        studentsOfTaoPaired.Add(studentOfTao);
                        break;
                    }
                }
            }

            string text = "", textClass = "correct";
            foreach ((string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email) student in students)
            {
                if (student.loginEmail == loginEmail)
                {
                    text = "Tento email již v systému existuje.";
                    textClass = "incorrect";
                    break;
                }
                else if (role == "0" && student.studentNumberIdentifier == studentNumberIdentifier)
                {
                    text = "Tento student je již spárován s emailem: " + student.loginEmail;
                    textClass = "incorrect";
                    break;
                }
            }

            if (role == "0" && studentNumberIdentifier == "")
            {
                text = "Při výběru role student je nutné vybrat studenta.";
                textClass = "incorrect";
            }
            else if (textClass == "correct")
            {
                try
                {
                    switch (role)
                    {
                        case "0":
                            studentController.EditUser(loginEmail, studentNumberIdentifier, int.Parse(role));
                            break;
                        default:
                            studentController.EditUser(loginEmail, "", int.Parse(role));
                            break;
                    }
                    return RedirectToAction("ManageUserList", "Home", new { text = "user_successfully_added" });
                }
                catch
                {
                    text = "Email obsahuje nepovolené znaky.";
                    textClass = "partiallyCorrect";
                }
            }

            return View(new ManageUserListModel
            {
                Title = "Správa uživatelů",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                Students = students,
                StudentsByRoles = studentsByRoles,
                StudentsOfTao = studentsOfTao,
                StudentsOfTaoNotPaired = studentsOfTao.Except(studentsOfTaoPaired).ToList(),
                RoleTexts = new string[] { "Student", "Učitel", "Správce" },
                LoginEmail = (textClass == "correct" ? "" : loginEmail),
                Role = (textClass == "correct" ? "" : role),
                StudentNumberIdentifier = (textClass == "correct" ? "" : studentNumberIdentifier),
                Text = text,
                TextClass = textClass
            });
        }

        [HttpGet]
        public IActionResult DeleteUser(string loginEmail)
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 2, 2)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }

            int role = studentController.LoadStudentByEmail(loginEmail).role;
            if (role >= 2)
            {
                List<(string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email)> students = studentController.LoadStudentsByEmail();
                int adminCount = 0;
                foreach ((string loginEmail, string studentNumberIdentifier, int role, string studentIdentifier, string login, string firstName, string lastName, string email) student in students)
                {
                    if (student.role >= 2)
                    {
                        if (adminCount + 1 > 1)
                        {
                            studentController.DeleteUser(loginEmail);
                            return RedirectToAction("ManageUserList", "Home", new { text = "user_successfully_deleted" });
                        }
                        adminCount++;
                    }
                }
                return RedirectToAction("ManageUserList", "Home", new { text = "last_admin_cannot_be_deleted" });
            }
            studentController.DeleteUser(loginEmail);
            return RedirectToAction("ManageUserList", "Home", new { text = "user_successfully_deleted" });
        }

        public IActionResult TeacherMenu()
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 1, 1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            return View(new PageModel
            {
                Title = "Učitel",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole
            });
        }

        public IActionResult TestTemplateList()
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 1, 1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            return View(new TestTemplateListModel
            {
                Title = "Správa zadání testů",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                Tests = testController.LoadTests()
            });
        }

        public IActionResult ManageSolvedTestList()
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 1, 1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            return View(new ManageSolvedTestListModel
            {
                Title = "Správa vyřešených testů",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                SolvedTests = testController.LoadSolvedTests()
            });
        }

        public IActionResult ManageSolvedTest(string testNameIdentifier, string testNumberIdentifier, string deliveryExecutionIdentifier, string studentIdentifier)
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 1, 1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            List<(string, string, string, string, int, bool)> itemParameters = testController.LoadItemInfo(testNameIdentifier, testNumberIdentifier);
            (List<(double questionResultPoints, bool questionResultPointsDetermined)> studentsPoints, int errorMessageNumber) questionResultPoints = testController.GetQuestionResultPoints(itemParameters, testNameIdentifier, testNumberIdentifier, deliveryExecutionIdentifier);

            return View(new ManageSolvedTestModel
            {
                Title = "Správa vyřešeného testu",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                TestNameIdentifier = testNameIdentifier,
                TestNumberIdentifier = testNumberIdentifier,
                DeliveryExecutionIdentifier = deliveryExecutionIdentifier,
                StudentIdentifier = studentIdentifier,
                TestParameters = testController.LoadTestParameters(testNameIdentifier, testNumberIdentifier),
                QuestionList = testController.LoadQuestions(testNameIdentifier, testNumberIdentifier),
                ItemParameters = itemParameters,
                TestPoints = testController.GetTestPoints(itemParameters),
                QuestionResultPoints = questionResultPoints,
                TotalStudentsPoints = testController.GetTotalStudentsPoints(questionResultPoints.studentsPoints),
                ResultParameters = testController.LoadResultParameters(testNameIdentifier, deliveryExecutionIdentifier, studentIdentifier)
            });
        }

        [HttpGet]
        public IActionResult TestTemplate(string testNameIdentifier, string testNumberIdentifier)
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 1, 1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            List<(string, string, string, string, int, bool)> itemParameters = testController.LoadItemInfo(testNameIdentifier, testNumberIdentifier);

            return View(new TestTemplateModel
            {
                Title = "Správa zadání testu",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                TestNameIdentifier = testNameIdentifier,
                TestNumberIdentifier = testNumberIdentifier,
                TestParameters = testController.LoadTestParameters(testNameIdentifier, testNumberIdentifier),
                QuestionList = testController.LoadQuestions(testNameIdentifier, testNumberIdentifier),
                ItemParameters = itemParameters,
                TestPoints = testController.GetTestPoints(itemParameters),
                NegativePoints = testController.NegativePointsManagement(testNameIdentifier, testNumberIdentifier)
            });
        }

        [HttpPost]
        public IActionResult TestTemplate(string testNameIdentifier, string testNumberIdentifier, string negativePoints)
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 1, 1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            string textToWrite = "";
            if (negativePoints == "negativePoints_no")
            {
                textToWrite = "0";
            }
            else if (negativePoints == "negativePoints_yes")
            {
                textToWrite = "1";
            }
            FileIO.WriteAllText(Settings.GetTestTestNegativePointsDataPath(testNameIdentifier, testNumberIdentifier), textToWrite);

            List<(string, string, string, string, int, bool)> itemParameters = testController.LoadItemInfo(testNameIdentifier, testNumberIdentifier);

            return View(new TestTemplateModel
            {
                Title = "Správa zadání testu",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                TestNameIdentifier = testNameIdentifier,
                TestNumberIdentifier = testNumberIdentifier,
                TestParameters = testController.LoadTestParameters(testNameIdentifier, testNumberIdentifier),
                QuestionList = testController.LoadQuestions(testNameIdentifier, testNumberIdentifier),
                ItemParameters = itemParameters,
                TestPoints = testController.GetTestPoints(itemParameters),
                NegativePoints = testController.NegativePointsManagement(testNameIdentifier, testNumberIdentifier),
                NegativePointsOption = negativePoints
            });
        }

        [HttpGet]
        public IActionResult ItemTemplate(string testNameIdentifier, string testNumberIdentifier, string itemNumberIdentifier, string itemNameIdentifier)
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 1, 1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            (string, string, string title, string label, int amountOfSubitems) itemParameters = itemController.LoadItemParameters(testNameIdentifier, itemNameIdentifier, itemNumberIdentifier);
            (List<string> responseIdentifierArray, List<string> responseValueArray, int errorMessageNumber) responseIdentifiers = itemController.GetResponseIdentifiers(itemParameters.amountOfSubitems, testNameIdentifier, itemNumberIdentifier);
            string responseIdentifier = responseIdentifiers.responseIdentifierArray[0];
            (string responseIdentifierTemp, int questionType, int subquestionPoints, bool subquestionPointsDetermined, double wrongChoicePoints, string imageSource, string subitemText, List<string> possibleAnswerArray, List<string> subquestionArray, List<string> correctChoiceArray, List<string> correctAnswerArray) subitemParameters = itemController.LoadSubitemParameters(responseIdentifier, itemParameters.amountOfSubitems, responseIdentifiers.responseIdentifierArray, responseIdentifiers.responseValueArray, testNameIdentifier, itemNumberIdentifier);
            double correctChoicePoints = itemController.GetCorrectChoicePoints(subitemParameters.subquestionPoints, subitemParameters.correctChoiceArray, subitemParameters.questionType);
            (bool recommendedWrongChoicePoints, double selectedWrongChoicePoints, int questionPoints, bool questionPointsDetermined) questionPoints = itemController.LoadQuestionPoints(testNameIdentifier, itemNumberIdentifier, responseIdentifier, itemParameters.amountOfSubitems, correctChoicePoints);

            return View(new ItemTemplateModel
            {
                Title = "Správa zadání otázky",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                TestNameIdentifier = testNameIdentifier,
                TestNumberIdentifier = testNumberIdentifier,
                ItemNameIdentifier = itemNameIdentifier,
                ItemNumberIdentifier = itemNumberIdentifier,
                ItemParameters = itemParameters,
                ResponseIdentifiers = responseIdentifiers,
                ResponseIdentifier = responseIdentifier,
                SubitemParameters = subitemParameters,
                QuestionPoints = questionPoints,
                QuestionTypeText = itemController.GetQuestionTypeText(subitemParameters.questionType),
                IsSelectDisabled = (itemParameters.amountOfSubitems > 1 ? false : true),
                CorrectChoicePoints = correctChoicePoints,
                CorrectChoiceArray = subitemParameters.correctChoiceArray,
                CorrectAnswerCount = (subitemParameters.questionType == 3 || subitemParameters.questionType == 4 ? subitemParameters.correctAnswerArray.Count / 2 : subitemParameters.correctAnswerArray.Count),
                WrongChoicePoints = subitemParameters.wrongChoicePoints,
                SubquestionPoints = subitemParameters.subquestionPoints
            });
        }

        [HttpPost]
        public IActionResult ItemTemplate(string testNameIdentifier, string testNumberIdentifier, string itemNumberIdentifier, string itemNameIdentifier, string selectedSubitem, string subquestionPoints,
            string wrongChoicePoints, string recommendedWrongChoicePoints, string selectedWrongChoicePoints, double correctChoicePoints, List<string> correctChoiceArray, int correctChoiceArrayCount, int correctChoiceArrayCountAlternative, int questionType)
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 1, 1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            if (questionType == 4 || questionType == 9)
            {
                correctChoiceArrayCount = correctChoiceArrayCountAlternative;
            }
            if (questionType == 4)
            {
                correctChoiceArrayCount *= 2;
            }
            if (correctChoiceArrayCount != correctChoiceArray.Count)
            {
                correctChoiceArray.Clear();
                for (int i = 0; i < correctChoiceArrayCount; i++)
                {
                    correctChoiceArray.Add("");
                }
            }

            string errorText = "";
            if (subquestionPoints != null)
            {
                bool isNumber = int.TryParse(subquestionPoints, out _);
                if (!isNumber)
                {
                    errorText = "Chyba: je nutné zadat číslo.";
                }
                else
                {
                    recommendedWrongChoicePoints = recommendedWrongChoicePoints.Replace('.', ',');
                    if (wrongChoicePoints == "wrongChoicePoints_recommended")
                    {
                        wrongChoicePoints = recommendedWrongChoicePoints;
                    }
                    else
                    {
                        wrongChoicePoints = selectedWrongChoicePoints;
                    }

                    if (recommendedWrongChoicePoints == "N/A")
                    {
                        wrongChoicePoints = (correctChoicePoints * (-1)).ToString();
                    }

                    wrongChoicePoints = wrongChoicePoints.Replace(".", ",");
                    bool isWrongChoicePointsNumber = double.TryParse(wrongChoicePoints, out _);

                    if (!isWrongChoicePointsNumber)
                    {
                        errorText = "Chyba: je nutné zadat číslo.";
                    }
                    else
                    {
                        bool performSave = true;
                        int subquestionPointsToSave = int.Parse(subquestionPoints);
                        double wrongChoicePointsToSave = double.Parse(wrongChoicePoints);

                        if (Math.Abs(wrongChoicePointsToSave) > subquestionPointsToSave)
                        {
                            performSave = false;
                            errorText = "Chyba: za špatný výběr bude studentovi odečteno více bodů, než kolik může dostat za otázku.";
                        }

                        if (wrongChoicePointsToSave > 0)
                        {
                            performSave = false;
                            errorText = "Chyba: za špatnou volbu nemůže být udělen kladný počet bodů.";
                        }

                        if (subquestionPointsToSave < 0)
                        {
                            performSave = false;
                            errorText = "Chyba: za správnou odpověď nemůže být udělen záporný počet bodů.";
                        }

                        if (performSave)
                        {
                            string[] importedFileLines = FileIO.ReadAllLines(Settings.GetTestItemPointsDataPath(testNameIdentifier, itemNumberIdentifier));
                            string fileLinesToExport = "";
                            for (int i = 0; i < importedFileLines.Length; i++)
                            {
                                string[] splitImportedFileLineBySemicolon = importedFileLines[i].Split(";");
                                if (splitImportedFileLineBySemicolon[0] == selectedSubitem)
                                {
                                    importedFileLines[i] = selectedSubitem + ";" + subquestionPointsToSave + ";" + Math.Round(wrongChoicePointsToSave, 2);
                                }
                                fileLinesToExport += importedFileLines[i] + "\n";
                            }
                            FileIO.WriteAllText(Settings.GetTestItemPointsDataPath(testNameIdentifier, itemNumberIdentifier), fileLinesToExport);
                        }
                    }
                }
            }
            (string, string, string title, string label, int amountOfSubitems) itemParameters = itemController.LoadItemParameters(testNameIdentifier, itemNameIdentifier, itemNumberIdentifier);
            (List<string> responseIdentifierArray, List<string> responseValueArray, int errorMessageNumber) responseIdentifiers = itemController.GetResponseIdentifiers(itemParameters.amountOfSubitems, testNameIdentifier, itemNumberIdentifier);
            string responseIdentifier = (itemParameters.amountOfSubitems == 1 || selectedSubitem == null ? responseIdentifiers.responseIdentifierArray[0] : selectedSubitem);
            (string responseIdentifierTemp, int questionType, int subquestionPoints, bool subquestionPointsDetermined, double wrongChoicePoints, string imageSource, string subitemText, List<string> possibleAnswerArray, List<string> subquestionArray, List<string> correctChoiceArray, List<string> correctAnswerArray) subitemParameters = itemController.LoadSubitemParameters(responseIdentifier, itemParameters.amountOfSubitems, responseIdentifiers.responseIdentifierArray, responseIdentifiers.responseValueArray, testNameIdentifier, itemNumberIdentifier);
            (bool recommendedWrongChoicePoints, double selectedWrongChoicePoints, int questionPoints, bool questionPointsDetermined) questionPoints = itemController.LoadQuestionPoints(testNameIdentifier, itemNumberIdentifier, responseIdentifier, itemParameters.amountOfSubitems, correctChoicePoints);

            ItemTemplateModel model = new ItemTemplateModel()
            {
                Title = "Správa zadání otázky",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                TestNameIdentifier = testNameIdentifier,
                TestNumberIdentifier = testNumberIdentifier,
                ItemNameIdentifier = itemNameIdentifier,
                ItemNumberIdentifier = itemNumberIdentifier
            };
            model.ItemParameters = itemParameters;
            model.ResponseIdentifiers = responseIdentifiers;
            model.ResponseIdentifier = responseIdentifier;
            model.SubitemParameters = subitemParameters;
            model.QuestionPoints = questionPoints;
            model.QuestionTypeText = itemController.GetQuestionTypeText(subitemParameters.questionType);
            model.IsSelectDisabled = (itemParameters.amountOfSubitems > 1 ? false : true);
            model.CorrectChoicePoints = itemController.GetCorrectChoicePoints(subitemParameters.subquestionPoints, subitemParameters.correctChoiceArray, subitemParameters.questionType);
            model.CorrectChoiceArray = correctChoiceArray;
            model.CorrectAnswerCount = (subitemParameters.questionType == 3 || subitemParameters.questionType == 4 ? subitemParameters.correctAnswerArray.Count / 2 : subitemParameters.correctAnswerArray.Count);
            if (wrongChoicePoints != null) { model.WrongChoicePoints = double.Parse(wrongChoicePoints); }
            else { model.WrongChoicePoints = subitemParameters.wrongChoicePoints; }
            if (subquestionPoints != null) { model.SubquestionPoints = int.Parse(subquestionPoints); }
            else { model.SubquestionPoints = subitemParameters.subquestionPoints; }
            model.SubquestionPointsText = subquestionPoints;
            model.ErrorText = errorText;

            return View(model);
        }

        [HttpGet]
        public IActionResult ManageSolvedItem(string testNameIdentifier, string testNumberIdentifier, string itemNumberIdentifier, string itemNameIdentifier, string deliveryExecutionIdentifier, string studentIdentifier)
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 1, 1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            (string, string, string title, string label, int amountOfSubitems) itemParameters = itemController.LoadItemParameters(testNameIdentifier, itemNameIdentifier, itemNumberIdentifier);
            (List<string> responseIdentifierArray, List<string> responseValueArray, int errorMessageNumber) responseIdentifiers = itemController.GetResponseIdentifiers(itemParameters.amountOfSubitems, testNameIdentifier, itemNumberIdentifier);
            string responseIdentifier = responseIdentifiers.responseIdentifierArray[0];
            (string responseIdentifierTemp, int questionType, int subquestionPoints, bool subquestionPointsDetermined, double wrongChoicePoints, string imageSource, string subitemText, List<string> possibleAnswerArray, List<string> subquestionArray, List<string> correctChoiceArray, List<string> correctAnswerArray) subitemParameters = itemController.LoadSubitemParameters(responseIdentifier, itemParameters.amountOfSubitems, responseIdentifiers.responseIdentifierArray, responseIdentifiers.responseValueArray, testNameIdentifier, itemNumberIdentifier);
            List<double> studentsSubitemPointsList = itemController.GetStudentsSubitemPointsList(testNameIdentifier, itemNameIdentifier, deliveryExecutionIdentifier);
            double correctChoicePoints = itemController.GetCorrectChoicePoints(subitemParameters.subquestionPoints, subitemParameters.correctChoiceArray, subitemParameters.questionType);
            (bool recommendedWrongChoicePoints, double selectedWrongChoicePoints, int questionPoints, bool questionPointsDetermined) questionPoints = itemController.LoadQuestionPoints(testNameIdentifier, itemNumberIdentifier, responseIdentifier, itemParameters.amountOfSubitems, correctChoicePoints);
            (double, List<string> studentsAnswers, string studentsAnswerCorrectLabel, string studentsAnswerPointsLabel) deliveryExecutionInfo = itemController.LoadDeliveryExecutionInfo(testNameIdentifier, testNumberIdentifier, itemNumberIdentifier, itemNameIdentifier, responseIdentifier, deliveryExecutionIdentifier, subitemParameters.correctAnswerArray, subitemParameters.correctChoiceArray, subitemParameters.subquestionPoints, questionPoints.recommendedWrongChoicePoints, questionPoints.selectedWrongChoicePoints, true, itemController.GetCurrentSubitemIndex(responseIdentifier, responseIdentifiers.responseIdentifierArray));
            string answerClass = "";
            switch (deliveryExecutionInfo.studentsAnswerCorrectLabel)
            {
                case "Správná odpověď":
                    answerClass = "correct";
                    break;
                case "Částečně správná odpověď":
                    answerClass = "partiallyCorrect";
                    break;
                case "Nesprávná odpověď":
                    answerClass = "incorrect";
                    break;
            }

            return View(new ManageSolvedItemModel
            {
                Title = "Správa vyřešené otázky",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                TestNameIdentifier = testNameIdentifier,
                TestNumberIdentifier = testNumberIdentifier,
                ItemNameIdentifier = itemNameIdentifier,
                ItemNumberIdentifier = itemNumberIdentifier,
                DeliveryExecutionIdentifier = deliveryExecutionIdentifier,
                StudentIdentifier = studentIdentifier,
                ItemParameters = itemParameters,
                ResponseIdentifiers = responseIdentifiers,
                ResponseIdentifier = responseIdentifier,
                SubitemParameters = subitemParameters,
                StudentsSubitemPointsList = studentsSubitemPointsList,
                StudentsSubitemPointsListSum = studentsSubitemPointsList.Sum(),
                StudentsSubitemPoints = itemController.GetStudentsSubitemPoints(studentsSubitemPointsList, responseIdentifier, responseIdentifiers.responseIdentifierArray),
                QuestionPoints = questionPoints,
                DeliveryExecutionInfo = deliveryExecutionInfo,
                AnswerClass = answerClass,
                QuestionTypeText = itemController.GetQuestionTypeText(subitemParameters.questionType),
                IsSelectDisabled = (itemParameters.amountOfSubitems > 1 ? false : true),
                CurrentSubitemIndex = itemController.GetCurrentSubitemIndex(responseIdentifier, responseIdentifiers.responseIdentifierArray)
            });
        }

        [HttpPost]
        public IActionResult ManageSolvedItem(string testNameIdentifier, string testNumberIdentifier, string itemNumberIdentifier, string itemNameIdentifier, string deliveryExecutionIdentifier, string studentIdentifier, string selectedSubitem, string studentsPoints, int subquestionMaxPoints, int amountOfSubitems, int subitemIndex, int questionPointsDetermined)
        {
            // Check if user can access page
            if (!CanUserAccessPage("", 1, 1)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            string errorText = "";
            if (studentsPoints != null)
            {
                bool isDecimal = double.TryParse(studentsPoints, NumberStyles.Number, CultureInfo.InvariantCulture, out _);
                double studentsPointsToSave = 0;
                if (isDecimal)
                {
                    studentsPointsToSave = double.Parse(studentsPoints, CultureInfo.InvariantCulture);
                }

                if (!isDecimal)
                {
                    errorText = "Chyba: je nutné zadat číslo.";
                }
                else if (questionPointsDetermined == 0)
                {
                    errorText = "Chyba: není možné upravit počet bodů studenta. Nejprve je nutné určit počet obdržených bodů za otázku.";
                }
                else if (studentsPointsToSave > subquestionMaxPoints)
                {
                    errorText = "Chyba: student nemůže za podotázku získat vyšší než maximální počet bodů (maximální počet bodů: " + subquestionMaxPoints.ToString() + ")";
                }
                else if (studentsPointsToSave < subquestionMaxPoints * (-1))
                {
                    errorText = "Chyba: student nemůže za podotázku získat nižší než minimální počet bodů (minimální počet bodů: " + (subquestionMaxPoints * (-1)).ToString() + ")";
                }
                else
                {
                    string[] resultsFileLines = FileIO.ReadAllLines(Settings.GetResultResultsDataPath(testNameIdentifier, deliveryExecutionIdentifier));
                    string resultsToFile = "";

                    for (int i = 0; i < resultsFileLines.Length; i++)
                    {
                        string[] splitResultsFileLineBySemicolon = resultsFileLines[i].Split(";");
                        if (splitResultsFileLineBySemicolon[0] != itemNameIdentifier)
                        {
                            resultsToFile += resultsFileLines[i] + "\n";
                        }
                        else
                        {
                            if (amountOfSubitems > 1)
                            {
                                resultsToFile += itemNameIdentifier;

                                for (int j = 1; j < splitResultsFileLineBySemicolon.Length; j++)
                                {
                                    resultsToFile += ";";
                                    if (j - 1 != subitemIndex)
                                    {
                                        resultsToFile += splitResultsFileLineBySemicolon[j];
                                    }
                                    else
                                    {
                                        resultsToFile += studentsPoints;
                                    }
                                }

                                resultsToFile += "\n";
                            }
                            else
                            {
                                resultsToFile += itemNameIdentifier + ";" + studentsPoints + "\n";
                            }
                        }
                    }

                    FileIO.WriteAllText(Settings.GetResultResultsDataPath(testNameIdentifier, deliveryExecutionIdentifier), resultsToFile);
                }
            }
            (string, string, string title, string label, int amountOfSubitems) itemParameters = itemController.LoadItemParameters(testNameIdentifier, itemNameIdentifier, itemNumberIdentifier);
            (List<string> responseIdentifierArray, List<string> responseValueArray, int errorMessageNumber) responseIdentifiers = itemController.GetResponseIdentifiers(itemParameters.amountOfSubitems, testNameIdentifier, itemNumberIdentifier);
            string responseIdentifier = (itemParameters.amountOfSubitems == 1 || selectedSubitem == null ? responseIdentifiers.responseIdentifierArray[0] : selectedSubitem);
            (string responseIdentifierTemp, int questionType, int subquestionPoints, bool subquestionPointsDetermined, double wrongChoicePoints, string imageSource, string subitemText, List<string> possibleAnswerArray, List<string> subquestionArray, List<string> correctChoiceArray, List<string> correctAnswerArray) subitemParameters = itemController.LoadSubitemParameters(responseIdentifier, itemParameters.amountOfSubitems, responseIdentifiers.responseIdentifierArray, responseIdentifiers.responseValueArray, testNameIdentifier, itemNumberIdentifier);
            List<double> studentsSubitemPointsList = itemController.GetStudentsSubitemPointsList(testNameIdentifier, itemNameIdentifier, deliveryExecutionIdentifier);
            double correctChoicePoints = itemController.GetCorrectChoicePoints(subitemParameters.subquestionPoints, subitemParameters.correctChoiceArray, subitemParameters.questionType);
            (bool recommendedWrongChoicePoints, double selectedWrongChoicePoints, int questionPoints, bool questionPointsDetermined) questionPoints = itemController.LoadQuestionPoints(testNameIdentifier, itemNumberIdentifier, responseIdentifier, itemParameters.amountOfSubitems, correctChoicePoints);
            (double, List<string> studentsAnswers, string studentsAnswerCorrectLabel, string studentsAnswerPointsLabel) deliveryExecutionInfo = itemController.LoadDeliveryExecutionInfo(testNameIdentifier, testNumberIdentifier, itemNumberIdentifier, itemNameIdentifier, responseIdentifier, deliveryExecutionIdentifier, subitemParameters.correctAnswerArray, subitemParameters.correctChoiceArray, subitemParameters.subquestionPoints, questionPoints.recommendedWrongChoicePoints, questionPoints.selectedWrongChoicePoints, true, itemController.GetCurrentSubitemIndex(responseIdentifier, responseIdentifiers.responseIdentifierArray));
            string answerClass = "";
            switch (deliveryExecutionInfo.studentsAnswerCorrectLabel)
            {
                case "Správná odpověď":
                    answerClass = "correct";
                    break;
                case "Částečně správná odpověď":
                    answerClass = "partiallyCorrect";
                    break;
                case "Nesprávná odpověď":
                    answerClass = "incorrect";
                    break;
            }

            return View(new ManageSolvedItemModel
            {
                Title = "Správa vyřešené otázky",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                TestNameIdentifier = testNameIdentifier,
                TestNumberIdentifier = testNumberIdentifier,
                ItemNameIdentifier = itemNameIdentifier,
                ItemNumberIdentifier = itemNumberIdentifier,
                DeliveryExecutionIdentifier = deliveryExecutionIdentifier,
                StudentIdentifier = studentIdentifier,
                ItemParameters = itemParameters,
                ResponseIdentifiers = responseIdentifiers,
                ResponseIdentifier = responseIdentifier,
                SubitemParameters = subitemParameters,
                StudentsSubitemPointsList = studentsSubitemPointsList,
                StudentsSubitemPointsListSum = studentsSubitemPointsList.Sum(),
                StudentsSubitemPoints = itemController.GetStudentsSubitemPoints(studentsSubitemPointsList, responseIdentifier, responseIdentifiers.responseIdentifierArray),
                QuestionPoints = questionPoints,
                DeliveryExecutionInfo = deliveryExecutionInfo,
                AnswerClass = answerClass,
                QuestionTypeText = itemController.GetQuestionTypeText(subitemParameters.questionType),
                IsSelectDisabled = (itemParameters.amountOfSubitems > 1 ? false : true),
                CurrentSubitemIndex = itemController.GetCurrentSubitemIndex(responseIdentifier, responseIdentifiers.responseIdentifierArray),
                StudentsPoints = studentsPoints,
                ErrorText = errorText
            });
        }

        public IActionResult BrowseSolvedTestList(string studentIdentifier)
        {
            // Check if user can access page
            if (!CanUserAccessPage(studentIdentifier, 0, 0)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            return View(new BrowseSolvedTestListModel
            {
                Title = "Prohlížení vyřešených testů",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                StudentIdentifier = studentIdentifier,
                Student = studentController.LoadStudentByIdentifier(studentIdentifier),
                StudentTestList = testController.LoadTests(studentIdentifier)
            });
        }

        public IActionResult BrowseSolvedTest(string studentIdentifier, string deliveryExecutionIdentifier, string testNameIdentifier, string testNumberIdentifier)
        {
            // Check if user can access page
            if (!CanUserAccessPage(studentIdentifier, 0, 0)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            List<(string, string, string, string, int, bool)> itemParameters = testController.LoadItemInfo(testNameIdentifier, testNumberIdentifier);
            (List<(double questionResultPoints, bool questionResultPointsDetermined)> studentsPoints, int errorMessageNumber) questionResultPoints = testController.GetQuestionResultPoints(itemParameters, testNameIdentifier, testNumberIdentifier, deliveryExecutionIdentifier);

            return View(new BrowseSolvedTestModel
            {
                Title = "Prohlížení vyřešeného testu",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                TestNameIdentifier = testNameIdentifier,
                TestNumberIdentifier = testNumberIdentifier,
                DeliveryExecutionIdentifier = deliveryExecutionIdentifier,
                StudentIdentifier = studentIdentifier,
                TestParameters = testController.LoadTestParameters(testNameIdentifier, testNumberIdentifier),
                QuestionList = testController.LoadQuestions(testNameIdentifier, testNumberIdentifier),
                ItemParameters = itemParameters,
                TestPoints = testController.GetTestPoints(itemParameters),
                QuestionResultPoints = questionResultPoints,
                TotalStudentsPoints = testController.GetTotalStudentsPoints(questionResultPoints.studentsPoints),
                ResultParameters = testController.LoadResultParameters(testNameIdentifier, deliveryExecutionIdentifier, studentIdentifier)
            });
        }

        [HttpGet]
        public IActionResult BrowseSolvedItem(string testNameIdentifier, string testNumberIdentifier, string itemNumberIdentifier, string itemNameIdentifier, string deliveryExecutionIdentifier, string studentIdentifier)
        {
            // Check if user can access page
            if (!CanUserAccessPage(studentIdentifier, 0, 0)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            (string, string, string title, string label, int amountOfSubitems) itemParameters = itemController.LoadItemParameters(testNameIdentifier, itemNameIdentifier, itemNumberIdentifier);
            (List<string> responseIdentifierArray, List<string> responseValueArray, int errorMessageNumber) responseIdentifiers = itemController.GetResponseIdentifiers(itemParameters.amountOfSubitems, testNameIdentifier, itemNumberIdentifier);
            string responseIdentifier = responseIdentifiers.responseIdentifierArray[0];
            (string responseIdentifierTemp, int questionType, int subquestionPoints, bool subquestionPointsDetermined, double wrongChoicePoints, string imageSource, string subitemText, List<string> possibleAnswerArray, List<string> subquestionArray, List<string> correctChoiceArray, List<string> correctAnswerArray) subitemParameters = itemController.LoadSubitemParameters(responseIdentifier, itemParameters.amountOfSubitems, responseIdentifiers.responseIdentifierArray, responseIdentifiers.responseValueArray, testNameIdentifier, itemNumberIdentifier);
            List<double> studentsSubitemPointsList = itemController.GetStudentsSubitemPointsList(testNameIdentifier, itemNameIdentifier, deliveryExecutionIdentifier);
            double correctChoicePoints = itemController.GetCorrectChoicePoints(subitemParameters.subquestionPoints, subitemParameters.correctChoiceArray, subitemParameters.questionType);
            (bool recommendedWrongChoicePoints, double selectedWrongChoicePoints, int questionPoints, bool questionPointsDetermined) questionPoints = itemController.LoadQuestionPoints(testNameIdentifier, itemNumberIdentifier, responseIdentifier, itemParameters.amountOfSubitems, correctChoicePoints);
            (double, List<string> studentsAnswers, string studentsAnswerCorrectLabel, string studentsAnswerPointsLabel) deliveryExecutionInfo = itemController.LoadDeliveryExecutionInfo(testNameIdentifier, testNumberIdentifier, itemNumberIdentifier, itemNameIdentifier, responseIdentifier, deliveryExecutionIdentifier, subitemParameters.correctAnswerArray, subitemParameters.correctChoiceArray, subitemParameters.subquestionPoints, questionPoints.recommendedWrongChoicePoints, questionPoints.selectedWrongChoicePoints, true, itemController.GetCurrentSubitemIndex(responseIdentifier, responseIdentifiers.responseIdentifierArray));
            string answerClass = "";
            switch (deliveryExecutionInfo.studentsAnswerCorrectLabel)
            {
                case "Správná odpověď":
                    answerClass = "correct";
                    break;
                case "Částečně správná odpověď":
                    answerClass = "partiallyCorrect";
                    break;
                case "Nesprávná odpověď":
                    answerClass = "incorrect";
                    break;
            }

            return View(new BrowseSolvedItemModel
            {
                Title = "Prohlížení vyřešené otázky",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                TestNameIdentifier = testNameIdentifier,
                TestNumberIdentifier = testNumberIdentifier,
                ItemNameIdentifier = itemNameIdentifier,
                ItemNumberIdentifier = itemNumberIdentifier,
                DeliveryExecutionIdentifier = deliveryExecutionIdentifier,
                StudentIdentifier = studentIdentifier,
                ItemParameters = itemParameters,
                ResponseIdentifiers = responseIdentifiers,
                ResponseIdentifier = responseIdentifier,
                SubitemParameters = subitemParameters,
                StudentsSubitemPointsList = studentsSubitemPointsList,
                StudentsSubitemPointsListSum = studentsSubitemPointsList.Sum(),
                StudentsSubitemPoints = itemController.GetStudentsSubitemPoints(studentsSubitemPointsList, responseIdentifier, responseIdentifiers.responseIdentifierArray),
                QuestionPoints = questionPoints,
                DeliveryExecutionInfo = deliveryExecutionInfo,
                AnswerClass = answerClass,
                QuestionTypeText = itemController.GetQuestionTypeText(subitemParameters.questionType),
                IsSelectDisabled = (itemParameters.amountOfSubitems > 1 ? false : true)
            });
        }

        [HttpPost]
        public IActionResult BrowseSolvedItem(string testNameIdentifier, string testNumberIdentifier, string itemNumberIdentifier, string itemNameIdentifier, string deliveryExecutionIdentifier, string studentIdentifier, string selectedSubitem)
        {
            // Check if user can access page
            if (!CanUserAccessPage(studentIdentifier, 0, 0)) { return RedirectToAction("Index", "Home", new { error = "access_denied" }); }
            int userRole = GetUserRole();

            (string, string, string title, string label, int amountOfSubitems) itemParameters = itemController.LoadItemParameters(testNameIdentifier, itemNameIdentifier, itemNumberIdentifier);
            (List<string> responseIdentifierArray, List<string> responseValueArray, int errorMessageNumber) responseIdentifiers = itemController.GetResponseIdentifiers(itemParameters.amountOfSubitems, testNameIdentifier, itemNumberIdentifier);
            string responseIdentifier = (itemParameters.amountOfSubitems == 1 || selectedSubitem == null ? responseIdentifiers.responseIdentifierArray[0] : selectedSubitem);
            (string responseIdentifierTemp, int questionType, int subquestionPoints, bool subquestionPointsDetermined, double wrongChoicePoints, string imageSource, string subitemText, List<string> possibleAnswerArray, List<string> subquestionArray, List<string> correctChoiceArray, List<string> correctAnswerArray) subitemParameters = itemController.LoadSubitemParameters(responseIdentifier, itemParameters.amountOfSubitems, responseIdentifiers.responseIdentifierArray, responseIdentifiers.responseValueArray, testNameIdentifier, itemNumberIdentifier);
            List<double> studentsSubitemPointsList = itemController.GetStudentsSubitemPointsList(testNameIdentifier, itemNameIdentifier, deliveryExecutionIdentifier);
            double correctChoicePoints = itemController.GetCorrectChoicePoints(subitemParameters.subquestionPoints, subitemParameters.correctChoiceArray, subitemParameters.questionType);
            (bool recommendedWrongChoicePoints, double selectedWrongChoicePoints, int questionPoints, bool questionPointsDetermined) questionPoints = itemController.LoadQuestionPoints(testNameIdentifier, itemNumberIdentifier, responseIdentifier, itemParameters.amountOfSubitems, correctChoicePoints);
            (double, List<string> studentsAnswers, string studentsAnswerCorrectLabel, string studentsAnswerPointsLabel) deliveryExecutionInfo = itemController.LoadDeliveryExecutionInfo(testNameIdentifier, testNumberIdentifier, itemNumberIdentifier, itemNameIdentifier, responseIdentifier, deliveryExecutionIdentifier, subitemParameters.correctAnswerArray, subitemParameters.correctChoiceArray, subitemParameters.subquestionPoints, questionPoints.recommendedWrongChoicePoints, questionPoints.selectedWrongChoicePoints, true, itemController.GetCurrentSubitemIndex(responseIdentifier, responseIdentifiers.responseIdentifierArray));
            string answerClass = "";
            switch (deliveryExecutionInfo.studentsAnswerCorrectLabel)
            {
                case "Správná odpověď":
                    answerClass = "correct";
                    break;
                case "Částečně správná odpověď":
                    answerClass = "partiallyCorrect";
                    break;
                case "Nesprávná odpověď":
                    answerClass = "incorrect";
                    break;
            }

            return View(new BrowseSolvedItemModel
            {
                Title = "Prohlížení vyřešené otázky",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                TestNameIdentifier = testNameIdentifier,
                TestNumberIdentifier = testNumberIdentifier,
                ItemNameIdentifier = itemNameIdentifier,
                ItemNumberIdentifier = itemNumberIdentifier,
                DeliveryExecutionIdentifier = deliveryExecutionIdentifier,
                StudentIdentifier = studentIdentifier,
                ItemParameters = itemParameters,
                ResponseIdentifiers = responseIdentifiers,
                ResponseIdentifier = responseIdentifier,
                SubitemParameters = subitemParameters,
                StudentsSubitemPointsList = studentsSubitemPointsList,
                StudentsSubitemPointsListSum = studentsSubitemPointsList.Sum(),
                StudentsSubitemPoints = itemController.GetStudentsSubitemPoints(studentsSubitemPointsList, responseIdentifier, responseIdentifiers.responseIdentifierArray),
                QuestionPoints = questionPoints,
                DeliveryExecutionInfo = deliveryExecutionInfo,
                AnswerClass = answerClass,
                QuestionTypeText = itemController.GetQuestionTypeText(subitemParameters.questionType),
                IsSelectDisabled = (itemParameters.amountOfSubitems > 1 ? false : true)
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            int userRole = GetUserRole();

            return View(new ErrorViewModel
            {
                Title = "Error",
                HeaderMessageData = GetHeaderMessageData(),
                UserRole = userRole,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}