using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// WorkOrder Repository 인터페이스
    /// </summary>
    public interface IWorkOrderRepository : IBaseRepository<WorkOrder>
    {
        // 고유 번호로 조회
        Task<WorkOrder?> GetByOrderNumberAsync(string orderNumber);

        // 상세 정보 포함 조회
        Task<WorkOrder?> GetWithDetailsAsync(Guid id);
        Task<WorkOrder?> GetWithExecutionsAsync(Guid id);

        // 상태별 조회
        Task<IEnumerable<WorkOrder>> GetByStatusAsync(WorkOrderStatus status);
        Task<IEnumerable<WorkOrder>> GetActiveOrdersAsync(); // Created, Scheduled, InProgress
        Task<IEnumerable<WorkOrder>> GetPendingOrdersAsync(); // Created, Scheduled

        // 우선순위별 조회
        Task<IEnumerable<WorkOrder>> GetHighPriorityOrdersAsync(int threshold = 70);

        // 날짜별 조회
        Task<IEnumerable<WorkOrder>> GetScheduledOrdersAsync(DateTime date);
        Task<IEnumerable<WorkOrder>> GetOrdersInDateRangeAsync(DateTime startDate, DateTime endDate);

        // 제품별 조회
        Task<IEnumerable<WorkOrder>> GetByProductAsync(Guid productId);

        // 시나리오별 조회
        Task<IEnumerable<WorkOrder>> GetByScenarioAsync(Guid scenarioId);

        // 현재 단계별 조회
        Task<IEnumerable<WorkOrder>> GetByCurrentStepAsync(int stepNumber);

        // 진행률 조회
        Task<IEnumerable<WorkOrder>> GetOrdersByProgressAsync(decimal minProgress, decimal maxProgress);

        // 다음 실행 가능한 작업 조회
        Task<WorkOrder?> GetNextExecutableOrderAsync();

        // 통계
        Task<Dictionary<WorkOrderStatus, int>> GetStatusStatisticsAsync();
        Task<int> GetCompletedOrdersCountAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetAverageCompletionTimeAsync(DateTime? startDate = null);

        // 번호 생성
        Task<string> GenerateOrderNumberAsync(string prefix = "WO");

        // 진행률 업데이트
        Task UpdateProgressAsync(Guid workOrderId, decimal progress);

        // 상태 변경
        Task<bool> UpdateStatusAsync(Guid workOrderId, WorkOrderStatus newStatus);

        // 현재 단계 업데이트
        Task UpdateCurrentStepAsync(Guid workOrderId, int stepNumber);
    }
}