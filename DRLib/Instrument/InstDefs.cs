namespace DRLib.Instrument;

public record Inst;

public interface IExpirable
{
    public DateTime ExpDt { get; }
}

public record FutureChain(string Und) : Inst;

public record Future(string Und, DateTime ExpDt) : FutureChain(Und), IExpirable;

public abstract record Option(string Und, double Strike, DateTime ExpDt, bool IsCall) : Inst;

public abstract record EuropeanOption(string Und, double Strike, DateTime ExpDt, bool IsCall) : Option(Und, Strike, ExpDt, IsCall);
public record EuroCall(string Und, double Strike, DateTime ExpDt) : EuropeanOption(Und, Strike, ExpDt, true);
public record EuroPut(string Und, double Strike, DateTime ExpDt) : EuropeanOption(Und, Strike, ExpDt, false);

public abstract record AmericanOption(string Und, double Strike, DateTime ExpDt, bool IsCall) : Option(Und, Strike, ExpDt, IsCall);
public record AmericanCall(string Und, double Strike, DateTime ExpDt) : AmericanOption(Und, Strike, ExpDt, true);
public record AmericanPut(string Und, double Strike, DateTime ExpDt) : AmericanOption(Und, Strike, ExpDt, false);

public abstract record DigitalOption(string Und, double Strike, DateTime ExpDt, bool IsCall) : Option(Und, Strike, ExpDt, IsCall);
public record DigiCall(string Und, double Strike, DateTime ExpDt) : DigitalOption(Und, Strike, ExpDt, true);
public record DigiPut(string Und, double Strike, DateTime ExpDt) : DigitalOption(Und, Strike, ExpDt, false);