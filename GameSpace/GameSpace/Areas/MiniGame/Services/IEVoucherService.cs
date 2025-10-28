using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IEVoucherService
    {
        Task<IEnumerable<Evoucher>> GetAllEVouchersAsync();
        Task<IEnumerable<Evoucher>> GetEVouchersByUserIdAsync(int userId);
        Task<IEnumerable<Evoucher>> GetUnusedEVouchersByUserIdAsync(int userId);
        Task<Evoucher?> GetEVoucherByIdAsync(int eVoucherId);
        Task<Evoucher?> GetEVoucherByCodeAsync(string eVoucherCode);
        Task<bool> CreateEVoucherAsync(Evoucher eVoucher);
        Task<bool> UpdateEVoucherAsync(Evoucher eVoucher);
        Task<bool> DeleteEVoucherAsync(int eVoucherId);
        Task<bool> UseEVoucherAsync(int eVoucherId);
        Task<bool> GrantEVoucherToUserAsync(int userId, int eVoucherTypeId);
        Task<int> GetEVoucherCountByUserIdAsync(int userId);
        Task<IEnumerable<EvoucherType>> GetAllEVoucherTypesAsync();
        Task<EvoucherType?> GetEVoucherTypeByIdAsync(int eVoucherTypeId);
    }
}

