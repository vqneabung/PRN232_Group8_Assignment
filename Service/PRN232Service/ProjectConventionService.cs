using Application.Enities;
using Application.Models;
using Application.UnitOfWork;
using AutoMapper;
using Service.IPRN232Service;
using System.IO.Compression;
using System.Text.Json;

namespace Service.PRN232Service
{
    public class ProjectConventionService : IProjectConventionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string _configFilePath;

        public ProjectConventionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "projectConvention.json");
        }

        public async Task<ProjectConventionResponse?> GetCurrentConventionAsync()
        {
            // Try to get from database first
            var convention = await _unitOfWork.ProjectConventionRepository.GetCurrentConventionAsync();
            
            if (convention != null)
            {
                return _mapper.Map<ProjectConventionResponse>(convention);
            }

            // If not in database, try to load from config file
            if (File.Exists(_configFilePath))
            {
                var jsonContent = await File.ReadAllTextAsync(_configFilePath);
                var config = JsonSerializer.Deserialize<ProjectConventionRequest>(jsonContent);
                
                if (config != null)
                {
                    // Save to database for future use
                    var newConvention = _mapper.Map<ProjectConvention>(config);
                    await _unitOfWork.ProjectConventionRepository.AddAsync(newConvention);
                    await _unitOfWork.SaveAsync();
                    
                    return _mapper.Map<ProjectConventionResponse>(newConvention);
                }
            }

            return null;
        }

        public async Task<ProjectConventionResponse> UpdateConventionAsync(ProjectConventionRequest request)
        {
            var convention = _mapper.Map<ProjectConvention>(request);
            var updatedConvention = await _unitOfWork.ProjectConventionRepository.UpdateConventionAsync(convention);
            await _unitOfWork.SaveAsync();

            // Also update the config file
            var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(_configFilePath, jsonContent);

            return _mapper.Map<ProjectConventionResponse>(updatedConvention);
        }

        public async Task<SolutionValidationResponse> ValidateSolutionNamingAsync(SolutionValidationRequest request)
        {
            var student = await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                return new SolutionValidationResponse
                {
                    IsValid = false,
                    Message = "Student not found",
                    StudentCode = string.Empty,
                    ExpectedFileName = string.Empty
                };
            }

            var convention = await GetCurrentConventionAsync();
            if (convention == null)
            {
                return new SolutionValidationResponse
                {
                    IsValid = false,
                    Message = "Project convention not configured",
                    StudentCode = student.StudentCode,
                    ExpectedFileName = string.Empty
                };
            }

            var expectedFileName = $"{convention.ExpectedSolutionPrefix}{student.StudentCode}{convention.ExpectedSolutionSuffix}";
            
            try
            {
                string? actualSolutionFile = null;

                // Check if it's a zip file that needs extraction
                if (request.SubmissionPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    var tempFolder = Path.Combine(Path.GetTempPath(), "ValidationTemp_" + Guid.NewGuid());
                    try
                    {
                        ZipFile.ExtractToDirectory(request.SubmissionPath, tempFolder);
                        
                        // Find .sln files in extracted directory
                        var slnFiles = Directory.GetFiles(tempFolder, "*.sln", SearchOption.AllDirectories);
                        if (slnFiles.Any())
                        {
                            actualSolutionFile = Path.GetFileName(slnFiles.First());
                        }
                    }
                    finally
                    {
                        if (Directory.Exists(tempFolder))
                        {
                            Directory.Delete(tempFolder, true);
                        }
                    }
                }
                else if (Directory.Exists(request.SubmissionPath))
                {
                    // Direct directory path
                    var slnFiles = Directory.GetFiles(request.SubmissionPath, "*.sln", SearchOption.AllDirectories);
                    if (slnFiles.Any())
                    {
                        actualSolutionFile = Path.GetFileName(slnFiles.First());
                    }
                }

                if (string.IsNullOrEmpty(actualSolutionFile))
                {
                    return new SolutionValidationResponse
                    {
                        IsValid = false,
                        Message = "No .sln file found in submission",
                        StudentCode = student.StudentCode,
                        ExpectedFileName = expectedFileName,
                        ActualFileName = null
                    };
                }

                var isValid = string.Equals(actualSolutionFile, expectedFileName, StringComparison.OrdinalIgnoreCase);
                
                return new SolutionValidationResponse
                {
                    IsValid = isValid,
                    Message = isValid 
                        ? "Solution file name is valid" 
                        : $"Solution file name does not match expected format. Expected: {expectedFileName}, Found: {actualSolutionFile}",
                    StudentCode = student.StudentCode,
                    ExpectedFileName = expectedFileName,
                    ActualFileName = actualSolutionFile,
                    FilePath = actualSolutionFile
                };
            }
            catch (Exception ex)
            {
                return new SolutionValidationResponse
                {
                    IsValid = false,
                    Message = $"Error validating solution: {ex.Message}",
                    StudentCode = student.StudentCode,
                    ExpectedFileName = expectedFileName
                };
            }
        }

        public async Task<SolutionValidationResponse> ValidateSolutionNamingFromFileAsync(SolutionFileValidationRequest request)
        {
            var student = await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                return new SolutionValidationResponse
                {
                    IsValid = false,
                    Message = "Student not found",
                    StudentCode = string.Empty,
                    ExpectedFileName = string.Empty
                };
            }

            var convention = await GetCurrentConventionAsync();
            if (convention == null)
            {
                return new SolutionValidationResponse
                {
                    IsValid = false,
                    Message = "Project convention not configured",
                    StudentCode = student.StudentCode,
                    ExpectedFileName = string.Empty
                };
            }

            var expectedFileName = $"{convention.ExpectedSolutionPrefix}{student.StudentCode}{convention.ExpectedSolutionSuffix}";
            
            if (request.SubmissionFile == null || request.SubmissionFile.Length == 0)
            {
                return new SolutionValidationResponse
                {
                    IsValid = false,
                    Message = "No file provided",
                    StudentCode = student.StudentCode,
                    ExpectedFileName = expectedFileName
                };
            }

            var tempFolder = Path.Combine(Path.GetTempPath(), "FileValidationTemp_" + Guid.NewGuid());
            try
            {
                Directory.CreateDirectory(tempFolder);
                var tempFilePath = Path.Combine(tempFolder, request.SubmissionFile.FileName);

                // Save uploaded file to temp location
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await request.SubmissionFile.CopyToAsync(stream);
                }

                string? actualSolutionFile = null;

                // Check if it's a zip file that needs extraction
                if (request.SubmissionFile.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    var extractPath = Path.Combine(tempFolder, "extracted");
                    ZipFile.ExtractToDirectory(tempFilePath, extractPath);
                    
                    // Find .sln files in extracted directory
                    var slnFiles = Directory.GetFiles(extractPath, "*.sln", SearchOption.AllDirectories);
                    if (slnFiles.Any())
                    {
                        actualSolutionFile = Path.GetFileName(slnFiles.First());
                    }
                }
                else if (request.SubmissionFile.FileName.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                {
                    // Direct .sln file
                    actualSolutionFile = request.SubmissionFile.FileName;
                }

                if (string.IsNullOrEmpty(actualSolutionFile))
                {
                    return new SolutionValidationResponse
                    {
                        IsValid = false,
                        Message = "No .sln file found in submission",
                        StudentCode = student.StudentCode,
                        ExpectedFileName = expectedFileName,
                        ActualFileName = request.SubmissionFile.FileName
                    };
                }

                var isValid = string.Equals(actualSolutionFile, expectedFileName, StringComparison.OrdinalIgnoreCase);
                
                return new SolutionValidationResponse
                {
                    IsValid = isValid,
                    Message = isValid 
                        ? "Solution file name is valid" 
                        : $"Solution file name does not match expected format. Expected: {expectedFileName}, Found: {actualSolutionFile}",
                    StudentCode = student.StudentCode,
                    ExpectedFileName = expectedFileName,
                    ActualFileName = actualSolutionFile,
                    FilePath = actualSolutionFile
                };
            }
            catch (Exception ex)
            {
                return new SolutionValidationResponse
                {
                    IsValid = false,
                    Message = $"Error validating solution: {ex.Message}",
                    StudentCode = student.StudentCode,
                    ExpectedFileName = expectedFileName
                };
            }
            finally
            {
                // Clean up temp files
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }
    }
}