using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Security.Cryptography;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyService;
        public CompanyService(ICompanyRepository companyService)
        {
            _companyService = companyService;

        }
        public async Task<ResponseDTO> CheckInvitationCode(string invitationCode)
        {
            if (string.IsNullOrEmpty(invitationCode))
            {
                return new ResponseDTO { Success = false, Message = "Invitation code cannot be empty." };
            }
            try
            {
                var invitation = await _companyService.GetInvitationsByInvitationCode(invitationCode);
                if (invitation == null)
                {
                    return new ResponseDTO { Success = false, Message = "Invalid invitation code. Cannot register your account" };
                }
                if (invitation.ExpirationDate < DateTime.UtcNow)
                {
                    return new ResponseDTO { Success = false, Message = "Invitation code has expired. Cannot register your account" };
                }
                return new ResponseDTO { Success = true, Message = "Invitation code is valid." };
            }
            catch (Exception ex)
            {
                return new ResponseDTO { Success = false, Message = ex.Message };
            }
        }
        public async Task<CompanyDTO> UpdateCompany(CompanyDTO companyDTO)
        {
            if (companyDTO == null)
                throw new ArgumentException("Company is null");

            try
            {
                await _companyService.UpdateCompanyAsync(companyDTO);
                return companyDTO;
                //return new ResponseDTO
                //{
                //    Success = true,
                //    Message = "Company has been updated"
                //};
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(companyDTO));
            }
        }
        public async Task<CompanyConfigDTO> SaveCompanyConfig(CompanyConfigDTO companyConfigDTO)
        {
            if (companyConfigDTO == null)
                throw new ArgumentException("Company Configuration is null");
            try
            {
                await _companyService.SaveCompanyConfigAsync(companyConfigDTO);
                return companyConfigDTO;
                //return new ResponseDTO
                //{
                //    Success = true,
                //    Message = "Configurations has been saved successfully"
                //};
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(companyConfigDTO));
                //return new ResponseDTO
                //{
                //    Success = false,
                //    Message = ex.Message
                //};
            }
        }
        public async Task<CompanyDTO> GetCompanyById(int id)
        {
            //if (string.IsNullOrEmpty(id))
            //    throw new ArgumentException("User is null", nameof(userId));
            try
            {
                var company = await _companyService.GetCompanyById(id);
                return company;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(id));
            }
        }

        public async Task<CompanyConfigDTO> GetCompanyConfig(int companyId)
        {
            if (companyId < 0)
                throw new ArgumentException("Company Id is null", nameof(companyId));
            try
            {
                var companyConfig = await _companyService.GetCompanyConfig(companyId);
                return companyConfig;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(companyId));
            }
        }

        public async Task<List<UserDTO>> GetTeamOfCompany(int companyId)
        {
            if (companyId == 0) throw new ArgumentException(nameof(companyId));
            try
            {
                var result = await _companyService.GetTeamOfCompany(companyId);
                return result;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(companyId));
            }
        }

        public async Task<List<InvitationsDTO>> GetInvitations(int companyId)
        {
            try
            {
                var result = await _companyService.GetInvitations(companyId);

                var updatedList = result.Select(inv => new InvitationsDTO
                {
                    CompanyId = inv.CompanyId,
                    InvitationCode = inv.InvitationCode,
                    ExpirationDate = inv.ExpirationDate,
                    ExpirationStatus = inv.ExpirationDate > DateTime.UtcNow
                        ? "Active"
                        : "Expired"
                }).ToList();

                return updatedList;
            }
            catch (Exception ex)
            {
                throw; // ❗ don't use "throw ex"
            }
        }

        public string GenerateSecureCode(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var data = new byte[length];

            using var crypto = RandomNumberGenerator.Create();
            crypto.GetBytes(data);

            return new string(data.Select(b => chars[b % chars.Length]).ToArray());
        }

        public async Task<InvitationsDTO> GenerateCodeAsync(int companyId, DateTime expiryDate)
        {
            try
            {
                var generatedCode = GenerateSecureCode();
                var dto = new InvitationsDTO
                {
                    CompanyId = companyId,
                    ExpirationDate = expiryDate,
                    InvitationCode = generatedCode,
                    
                };
                var code =  await _companyService.GenerateCodeAsync(dto);
                var updatedList = new InvitationsDTO
                {
                    CompanyId = code.CompanyId,
                    ExpirationDate = code.ExpirationDate,
                    InvitationCode = code.InvitationCode,
                    ExpirationStatus = code.ExpirationDate > DateTime.UtcNow
                        ? "Active"
                        : "Expired"
                };
                return updatedList;
            }
            catch (Exception ex) {
                throw ex;
            }
        }
    }
}
