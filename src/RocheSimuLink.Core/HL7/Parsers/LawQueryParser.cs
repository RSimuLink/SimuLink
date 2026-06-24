using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Parsers
{
    /// <summary>
    /// Parses x800DM IHE-LAW query-flow messages back into their models:
    /// QBP^Q11 work order queries and RSP^K11 query responses. Symmetric to
    /// <see cref="Law.LawQbpQ11Builder"/> / <see cref="Law.LawRspK11Builder"/>.
    /// </summary>
    public static class LawQueryParser
    {
        public static WorkOrderQuery ParseQuery(string rawMessage) =>
            ParseQuery(Hl7Parser.Parse(rawMessage));

        public static WorkOrderQuery ParseQuery(ParsedHl7Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var query = new WorkOrderQuery
            {
                Header = LawSegmentReader.ReadHeader(message),
            };

            var qpd = message.Segment("QPD");
            if (qpd is not null)
            {
                query.QueryName = LawSegmentReader.ReadCoded(qpd, 1);
                query.QueryTag = qpd.Field(2);
                query.SampleId = qpd.Field(3);
                query.CarrierId = qpd.Field(4);
                query.CarrierPosition = qpd.Field(5);
            }

            var rcp = message.Segment("RCP");
            if (rcp is not null)
            {
                query.QueryPriority = rcp.Field(1);
                query.ResponseModality = LawSegmentReader.ReadCoded(rcp, 3);
            }

            return query;
        }

        public static WorkOrderQueryResponse ParseResponse(string rawMessage) =>
            ParseResponse(Hl7Parser.Parse(rawMessage));

        public static WorkOrderQueryResponse ParseResponse(ParsedHl7Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var response = new WorkOrderQueryResponse
            {
                Header = LawSegmentReader.ReadHeader(message),
            };

            var msa = message.Segment("MSA");
            if (msa is not null)
            {
                response.AcknowledgmentCode = msa.Field(1);
                response.AcknowledgedControlId = msa.Field(2);
            }

            var qak = message.Segment("QAK");
            if (qak is not null)
            {
                response.QueryTag = qak.Field(1);
                response.QueryResponseStatus = qak.Field(2);
                response.QueryName = LawSegmentReader.ReadCoded(qak, 3);
            }

            var qpd = message.Segment("QPD");
            if (qpd is not null)
            {
                response.EchoedQueryTag = qpd.Field(2);
                response.SampleId = qpd.Field(3);
                response.CarrierId = qpd.Field(4);
                response.CarrierPosition = qpd.Field(5);
            }

            return response;
        }
    }
}
