using QuickFuzzr;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.Tests;

public class TicketGenerator
{
    // [Fact]
    public void Go()
    {
        var indexFuzzr =
            from index in Fuzzr.Counter("ticket")
            select index.ToString().PadLeft(2, '0');

        var infoFuzzr =
            from index in indexFuzzr
            from orderId in Fuzzr.Guid()
            from code in Fuzzr.Guid()
            from ticketId in Fuzzr.Guid()
            select new Info(index, orderId, code, ticketId);

        var linkFlow =
            from info in Pulse.Start<Info>()
            from _1 in Pulse.TraceTo<TicketLinkLog>($"-- {info.Index} ----------------------------------------------------------------------------------------------------------------")
            from _2 in Pulse.TraceTo<TicketLinkLog>($" https://pequivents-hgm.netlify.app/tickets/{info.OrderId}_{info.Code}")
            from _3 in Pulse.TraceTo<TicketLinkLog>($"----------------------------------------------------------------------------------------------------------------------")
            from _4 in Pulse.TraceTo<TicketLinkLog>("")
            select info;

        var dbFlow =
            from info in Pulse.Start<Info>()
            from _1 in Pulse.Trace($"-- {info.Index} ----------------------------------------------------------------------------------------------------------------")
            from _2 in Pulse.Trace($"order:{info.OrderId}")
            from lb in Pulse.Trace("{")
            from id in Pulse.Trace($"\"id\": \"{info.OrderId}\",")
            from code in Pulse.Trace($"\"code\": \"{info.Code}\",")
            from christianName in Pulse.Trace($"\"christianName\": \"N/A\",")
            from lastName in Pulse.Trace($"\"lastName\": \"N/A\",")
            from email in Pulse.Trace($"\"email\": \"N/A\",")
            from total in Pulse.Trace($"\"total\": 0,")
            from tickets in Pulse.Trace($"\"tickets\": [{{\"id\": \"{info.TicketId}\", \"description\": \"Gratis Ticket\", \"price\": 0 }}]")
            from rb in Pulse.Trace("}")
            from _3 in Pulse.Trace($"----------------------------------------------------------------------------------------------------------------------")
            from _4 in Pulse.Trace("")
            select info;

        var flow =
            from info in Pulse.Start<Info>()
            from _1 in Pulse.ToFlow(linkFlow, info)
            from _2 in Pulse.ToFlow(dbFlow, info)
            select info;

        Signal.From(flow)
            .SetArtery(FileLog.Write("TicketsDb.json"))
            .Graft(new TicketLinkLog("TicketLinks.txt"))
            .Pulse(infoFuzzr.Many(40).Generate());
    }

    public record Info(string Index, Guid OrderId, Guid Code, Guid TicketId);
    public class TicketLinkLog(string filename) : FileLogArtery(filename);
}