namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 작업 지시 상태
    /// </summary>
    public enum WorkOrderStatus
    {
        Created = 0,      // 생성됨
        Scheduled = 1,    // 예약됨
        InProgress = 2,   // 진행중
        Completed = 3,    // 완료
        Cancelled = 4,    // 취소됨
        Failed = 5,       // 실패
        OnHold = 6        // 보류
    }

    /// <summary>
    /// 작업 실행 상태
    /// </summary>
    public enum ExecutionStatus
    {
        Pending = 0,      // 대기중
        InProgress = 1,   // 진행중
        Completed = 2,    // 완료
        Failed = 3,       // 실패
        Skipped = 4,      // 건너뜀
        Cancelled = 5     // 취소됨
    }

    /// <summary>
    /// 시나리오 단계 타입
    /// </summary>
    public enum StepType
    {
        AMRMove,          // AMR 이동
        RobotWork,        // 로봇 작업
        EquipmentWork,    // 설비 작업
        Wait,             // 대기
        Decision,         // 조건 분기
        Parallel          // 병렬 처리
    }

    /// <summary>
    /// 대상 시스템
    /// </summary>
    public enum TargetSystem
    {
        MES,              // MES 자체
        AMS,              // AMR Management System
        EMS,              // Equipment Management System
        ARMS              // Articulated Robot Management System
    }

    /// <summary>
    /// 작업 액션 타입
    /// </summary>
    public enum ActionType
    {
        // AMR 관련
        Move,             // 이동
        Load,             // 적재
        Unload,           // 하역
        StationLoad,      // 스테이션으로 적재
        StationUnload,    // 스테이션에서 하역
        ChargeMove,       // 충전소 이동

        // 설비 관련
        Process,          // 가공
        Inspect,          // 검사
        Transfer,         // 이송

        // 시스템 관련
        Notify,           // 알림
        WaitForSignal,    // 신호 대기
        CheckCondition    // 조건 확인
    }

    /// <summary>
    /// 다음 단계 진행 조건
    /// </summary>
    public enum NextStepCondition
    {
        OnComplete,       // 완료 시
        OnAnyComplete,    // 하나라도 완료 시
        OnAllComplete,    // 모두 완료 시
        OnCondition,      // 특정 조건 만족 시
        Immediate         // 즉시
    }

    /// <summary>
    /// 로그 레벨
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }

    /// <summary>
    /// AMR 타입
    /// </summary>
    public enum AMRType
    {
        Thira,            // 티라 AMR (다관절 로봇 탑재)
        Agilox            // 아길록스 AMR (팔레트 운송)
    }

    /// <summary>
    /// 팔레트 상태 열거형
    /// </summary>
    public enum PalletStatus
    {
        /// <summary>사용 가능</summary>
        Available = 1,
        /// <summary>사용 중</summary>
        InUse = 2,
        /// <summary>빈 상태 (회수 대기)</summary>
        Empty = 3,
        /// <summary>회수됨</summary>
        Collected = 4,
        /// <summary>점검 중</summary>
        Inspection = 5,
        /// <summary>수리 중</summary>
        Repair = 6,
        /// <summary>폐기</summary>
        Disposed = 7
    }

    /// <summary>
    /// 위치 타입 열거형
    /// </summary>
    public enum LocationType
    {
        /// <summary>생산 라인</summary>
        ProductionLine = 1,
        /// <summary>창고</summary>
        Warehouse = 2,
        /// <summary>임시 저장소</summary>
        TemporaryStorage = 3,
        /// <summary>수리 구역</summary>
        RepairArea = 4,
        /// <summary>폐기 구역</summary>
        DisposalArea = 5
    }
}