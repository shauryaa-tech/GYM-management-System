namespace GymManagement.Data.Queries
{
    public static class PaymentTransactionQueries
    {
        public const string GetByOrderId = @"
        SELECT TOP 1 * FROM PaymentTransactions WHERE OrderId = @OrderId";

        public const string Insert = @"
        INSERT INTO PaymentTransactions
        (
            MemberId, OrderId, TransactionId, Gateway, Amount, Currency,
            PaymentFor, Status, ResponseCode, ResponseMessage, GatewayResponse,
            PaidOn, CreatedDate
        )
        VALUES
        (
            @MemberId, @OrderId, @TransactionId, @Gateway, @Amount, @Currency,
            @PaymentFor, @Status, @ResponseCode, @ResponseMessage, @GatewayResponse,
            @PaidOn, @CreatedDate
        )";

        public const string Update = @"
        UPDATE PaymentTransactions SET
            MemberId = @MemberId,
            TransactionId = @TransactionId,
            Gateway = @Gateway,
            Amount = @Amount,
            Currency = @Currency,
            PaymentFor = @PaymentFor,
            Status = @Status,
            ResponseCode = @ResponseCode,
            ResponseMessage = @ResponseMessage,
            GatewayResponse = @GatewayResponse,
            PaidOn = @PaidOn
        WHERE Id = @Id";
    }
}
