using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace PcxLibrary.Tests;

internal static class TestUtils
{
    private static int dispatchCounter = 0;
    private static readonly List<Action<object, EventArgs>> dispatchs = new();

    public static object New(Type? type, params object[] values)
    {
        Assert.IsNotNull(type, "Unable to construct object of unknown type");

        if (values.Length == 0)
            return New(type);

        Type[] types = new Type[values.Length];
        for (int i = 0; i < values.Length; i++)
            types[i] = values[i].GetType();

        var constructor = type?.GetConstructor(types);
        Assert.IsNotNull(constructor, $"Class {type.Name} is missing parametric constructor");

        object? obj = constructor?.Invoke(values);
        Assert.IsNotNull(obj, $"Class {type.Name} failed to construct object using parametric constructor");
        return obj;
    }

    private static object New(Type? type)
    {
        var constructor = type?.GetConstructor(new Type[0]);
        Assert.IsNotNull(constructor, $"Class {type.Name} is missing nonparametric constructor");

        object? obj = constructor?.Invoke(new object?[0]);
        Assert.IsNotNull(obj, $"Class {type.Name} failed to construct object using nonparametric constructor");
        return obj;
    }

    public static void RegisterEvent(object obj, string evt, Action<object, EventArgs> method)
    {
        var ev = obj.GetType().GetEvent(evt);
        Assert.IsNotNull(ev);

        var addMethod = ev.GetAddMethod();

        Type tDelegate = addMethod.GetParameters()[0].ParameterType;

        Type returnType = GetDelegateReturnType(tDelegate);
        if (returnType != typeof(void))
            throw new ArgumentException("Delegate has a return type.");

        DynamicMethod handler = new DynamicMethod("", null, GetDelegateParameterTypes(tDelegate));

        MethodInfo helper = typeof(TestUtils).GetMethod("HelperDispatcher");

        ILGenerator ilgen = handler.GetILGenerator();
        ilgen.Emit(OpCodes.Ldarg_0);
        ilgen.Emit(OpCodes.Ldarg_1);
        ilgen.Emit(OpCodes.Ldc_I4, dispatchCounter);
        ilgen.Emit(OpCodes.Call, helper);
        ilgen.Emit(OpCodes.Ret);

        dispatchs.Add(method);
        dispatchCounter++;

        Delegate del = handler.CreateDelegate(tDelegate);

        addMethod.Invoke(obj, new object?[] { del });
    }

    public static void HelperDispatcher(object a, EventArgs e, int c)
    {
        dispatchs[c](a, e);
    }

    private static Type[] GetDelegateParameterTypes(Type d)
    {
        if (d.BaseType != typeof(MulticastDelegate))
            throw new ArgumentException("Not a delegate.", nameof(d));

        MethodInfo invoke = d.GetMethod("Invoke");
        if (invoke == null)
            throw new ArgumentException("Not a delegate.", nameof(d));

        ParameterInfo[] parameters = invoke.GetParameters();
        Type[] typeParameters = new Type[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            typeParameters[i] = parameters[i].ParameterType;
        }
        return typeParameters;
    }

    private static Type GetDelegateReturnType(Type d)
    {
        if (d.BaseType != typeof(MulticastDelegate))
            throw new ArgumentException("Not a delegate.", nameof(d));

        MethodInfo invoke = d.GetMethod("Invoke");
        if (invoke == null)
            throw new ArgumentException("Not a delegate.", nameof(d));

        return invoke.ReturnType;
    }

    public static object? GetProperty(object obj, string prop)
    {
        var p = obj.GetType().GetProperty(prop);
        Assert.IsNotNull(p);

        var get = p.GetGetMethod();
        Assert.IsNotNull(get);

        return get.Invoke(obj, new object?[0]);
    }

    public static object? Invoke(object obj, string method, params object[] values)
    {
        var m = obj.GetType().GetMethod(method);
        Assert.IsNotNull(m);

        return m.Invoke(obj, values);
    }

    public static object? InvokePrivateStatic(object obj, string method, params object[] values)
    {
        var m = obj.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(m);

        return m.Invoke(obj, values);
    }

    public static object? IndexBy(object obj, object index)
    {
        var indexer = obj.GetType().GetProperty("Item");
        Assert.IsNotNull(indexer);

        var get = indexer.GetGetMethod();
        Assert.IsNotNull(get);

        return get.Invoke(obj, new object?[] { index });
    }

    public static object? SetIndexBy(object obj, object index, object value)
    {
        var indexer = obj.GetType().GetProperty("Item");
        Assert.IsNotNull(indexer);

        var set = indexer.GetSetMethod();
        Assert.IsNotNull(set);

        return set.Invoke(obj, new object?[] { index, value });
    }

    public static void AssertByEnumerator(object obj, params object[] values)
    {
        IEnumerator? enumerable = (IEnumerator)Invoke(obj, "GetEnumerator");
        List<object?> actual = new();
        while (enumerable.MoveNext())
            actual.Add(enumerable.Current);

        CollectionAssert.AreEqual(values, actual);
    }
}
