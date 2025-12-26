using QuickCheckr.Tests.Experimental.W_Warehouse.A_StepOne;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Experimental.W_Warehouse;

[DocFile]
[DocContent("> An evolving playground to examine some ideas.")]
[DocContent(
@"**High level ideas**:  
* Keep a number of 'clients' in a pool calling a web api (or service in this case for ease of coding)
  *RESTy*, they get products => use it to place order f.i., client object keeps track of what happened
  Could be used to verify concurrency issues et al, client asks data, places order, another client snatched last item ...
")]
[DocInclude(typeof(WarehouseStepOne))]
public class Warehousing
{
}