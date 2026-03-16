using QuickCheckr.Tests.Experimental.W_Warehouse.A_StepOne;
using QuickCheckr.Tests.Experimental.W_Warehouse.B_StepTwo;
using QuickCheckr.Tests.Experimental.W_Warehouse.C_StepThree;
using QuickCheckr.Tests.Experimental.W_Warehouse.D_StepFour;
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
[DocInclude(typeof(WarehouseStepTwo))]
[DocInclude(typeof(WarehouseStepThree))]
[DocInclude(typeof(WarehouseStepFour))]
public class Warehousing
{
}