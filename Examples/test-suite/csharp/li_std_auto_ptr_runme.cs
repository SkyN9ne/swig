using System;
using li_std_auto_ptrNamespace;

public class li_std_auto_ptr_runme {
    private static void WaitForGC()
    {
        System.GC.Collect(); 
        System.GC.WaitForPendingFinalizers();
        System.Threading.Thread.Sleep(10);
    }

    private static void checkCount(int expected_count)
    {
      int actual_count = Klass.getTotal_count();
      if (actual_count != expected_count)
        throw new ApplicationException("Counts incorrect, expected:" + expected_count + " actual:" + actual_count);
    }

    public static void Main()
    {
        // auto_ptr as input
        using (Klass kin = new Klass("KlassInput")) {
          checkCount(1);
          string s = li_std_auto_ptr.takeKlassAutoPtr(kin);
          checkCount(0);
          if (s != "KlassInput")
            throw new ApplicationException("Incorrect string: " + s);
          if (!li_std_auto_ptr.is_nullptr(kin))
            throw new ApplicationException("is_nullptr failed");
        } // Dispose should not fail, even though already deleted
        checkCount(0);

        using (Klass kin = new Klass("KlassInput")) {
          checkCount(1);
          string s = li_std_auto_ptr.takeKlassAutoPtr(kin);
          checkCount(0);
          if (s != "KlassInput")
            throw new ApplicationException("Incorrect string: " + s);
          if (!li_std_auto_ptr.is_nullptr(kin))
            throw new ApplicationException("is_nullptr failed");
          bool exception_thrown = false;
          try {
            li_std_auto_ptr.takeKlassAutoPtr(kin);
          } catch (ApplicationException e) {
            if (!e.Message.Contains("Cannot release ownership as memory is not owned"))
              throw new ApplicationException("incorrect exception message");
            exception_thrown = true;
          }
          if (!exception_thrown)
              throw new ApplicationException("double usage of takeKlassAutoPtr should have been an error");
        } // Dispose should not fail, even though already deleted
        checkCount(0);

        using (Klass kin = new Klass("KlassInput")) {
            bool exception_thrown = false;
            try {
              Klass notowned = li_std_auto_ptr.get_not_owned_ptr(kin);
              li_std_auto_ptr.takeKlassAutoPtr(notowned);
            } catch (ApplicationException e) {
              if (!e.Message.Contains("Cannot release ownership as memory is not owned"))
                throw new ApplicationException("incorrect exception message");
              exception_thrown = true;
            }
            if (!exception_thrown)
                throw new ApplicationException("Should have thrown 'Cannot release ownership as memory is not owned' error");
        }
        checkCount(0);

        using (KlassInheritance kini = new KlassInheritance("KlassInheritanceInput")) {
          checkCount(1);
          string s = li_std_auto_ptr.takeKlassAutoPtr(kini);
          checkCount(0);
          if (s != "KlassInheritanceInput")
            throw new ApplicationException("Incorrect string: " + s);
          if (!li_std_auto_ptr.is_nullptr(kini))
            throw new ApplicationException("is_nullptr failed");
        } // Dispose should not fail, even though already deleted
        checkCount(0);

        // auto_ptr as output
        Klass k1 = li_std_auto_ptr.makeKlassAutoPtr("first");
        if (k1.getLabel() != "first")
            throw new Exception("wrong object label");

        Klass k2 = li_std_auto_ptr.makeKlassAutoPtr("second");
        if (Klass.getTotal_count() != 2)
            throw new Exception("number of objects should be 2");

        using (Klass k3 = li_std_auto_ptr.makeKlassAutoPtr("second")) {
          if (Klass.getTotal_count() != 3)
            throw new Exception("number of objects should be 3");
        }
        if (Klass.getTotal_count() != 2)
            throw new Exception("number of objects should be 2");

        k1 = null;
        {
          int countdown = 500;
          int expectedCount = 1;
          while (true) {
            WaitForGC();
            if (--countdown == 0)
              break;
            if (Klass.getTotal_count() == expectedCount)
              break;
          };
          int actualCount = Klass.getTotal_count();
          if (actualCount != expectedCount)
            Console.Error.WriteLine("Expected count: " + expectedCount + " Actual count: " + actualCount); // Finalizers are not guaranteed to be run and sometimes they just don't
        }

        if (k2.getLabel() != "second")
            throw new Exception("wrong object label");

        k2 = null;
        {
          int countdown = 500;
          int expectedCount = 0;
          while (true) {
            WaitForGC();
            if (--countdown == 0)
              break;
            if (Klass.getTotal_count() == expectedCount)
              break;
          }
          int actualCount = Klass.getTotal_count();
          if (actualCount != expectedCount)
            Console.Error.WriteLine("Expected count: " + expectedCount + " Actual count: " + actualCount); // Finalizers are not guaranteed to be run and sometimes they just don't
        }
    }
}
