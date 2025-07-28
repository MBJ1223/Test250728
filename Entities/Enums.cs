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
        DisposalArea = 5,
        /// <summary>적재 구역</summary>
        LoadingZone = 6,
        /// <summary>하역 구역</summary>
        UnloadingZone = 7,
        /// <summary>대기 구역</summary>
        WaitingZone = 8
    }

    /// <summary>
    /// 제품 타입
    /// </summary>
    public enum ProductType
    {
        None,
        /// <summary>용접 제품</summary>
        WeldingProduct,
        /// <summary>볼팅 제품</summary>
        BoltingProduct,
        /// <summary>용접 원자재</summary>
        WeldingMaterial,
        /// <summary>볼팅 원자재</summary>
        BoltingMaterial
    }

    /// <summary>
    /// 공정 타입
    /// </summary>
    public enum ProcessType
    {
        /// <summary>입고</summary>
        Inbound,
        /// <summary>운송</summary>
        Transport,
        /// <summary>가공</summary>
        Process,
        /// <summary>검사</summary>
        Inspection,
        /// <summary>대기</summary>
        Wait,
        /// <summary>출고</summary>
        Outbound
    }

    /// <summary>
    /// 스테이션 타입
    /// </summary>
    public enum StationType
    {
        /// <summary>용접 스테이션</summary>
        WeldingStation,
        /// <summary>볼팅 스테이션</summary>
        BoltingStation,
        /// <summary>검사 스테이션</summary>
        InspectionStation,
        /// <summary>적재 구역</summary>
        LoadingZone,
        /// <summary>하역 구역</summary>
        UnloadingZone,
        /// <summary>대기 구역</summary>
        WaitingZone
    }

    /// <summary>
    /// 재고 상태
    /// </summary>
    public enum StockStatus
    {
        /// <summary>사용 가능</summary>
        Available,
        /// <summary>예약됨</summary>
        Reserved,
        /// <summary>작업 중</summary>
        InProcess,
        /// <summary>완료</summary>
        Completed,
        /// <summary>보류</summary>
        OnHold,
        /// <summary>불량</summary>
        Defective,
        /// <summary>출고됨</summary>
        Shipped
    }

    /// <summary>
    /// 품질 상태
    /// </summary>
    public enum QualityStatus
    {
        /// <summary>양호</summary>
        Good,
        /// <summary>재작업 필요</summary>
        NeedRework,
        /// <summary>불량</summary>
        Defective,
        /// <summary>검사 대기</summary>
        PendingInspection
    }

    /// <summary>
    /// 통합 타입
    /// </summary>
    public enum IntegrationType
    {
        REST,           // REST API
        MessageQueue,   // RabbitMQ 등
        SignalR,        // SignalR 실시간 통신
        RedisStream,    // Redis Streams
        gRPC            // gRPC
    }

    /// <summary>
    /// 통합 상태
    /// </summary>
    public enum IntegrationStatus
    {
        Active,         // 활성
        Inactive,       // 비활성
        Error,          // 오류
        Maintenance     // 유지보수
    }

    /// <summary>
    /// 통합 결과
    /// </summary>
    public enum IntegrationResult
    {
        Success,        // 성공
        Failed,         // 실패
        Timeout,        // 타임아웃
        Cancelled       // 취소됨
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
}