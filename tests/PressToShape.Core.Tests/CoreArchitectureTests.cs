using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace PressToShape.Core.Tests;

public class CoreArchitectureTests
{
    private static readonly Assembly CoreAssembly = Assembly.Load("PressToShape.Core");

    [Fact]
    public void CoreAssembly_IsLoadable()
    {
        Assert.Equal("PressToShape.Core", CoreAssembly.GetName().Name);
    }

    [Fact]
    public void Core_References_NoAvaloniaAssembly()
    {
        var offenders = CoreAssembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(n => n.StartsWith("Avalonia", System.StringComparison.Ordinal))
            .ToList();

        Assert.True(offenders.Count == 0, "Core must not reference Avalonia: " + string.Join(", ", offenders));
    }

    [Fact]
    public void Core_References_NoOtherSolutionProject()
    {
        var offenders = CoreAssembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(n => n.StartsWith("PressToShape.", System.StringComparison.Ordinal))
            .ToList();

        Assert.True(offenders.Count == 0, "Core must not reference other solution projects: " + string.Join(", ", offenders));
    }

    [Fact]
    public void Clipper2_IsUsedOnlyByBooleanEngine()
    {
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        var offenders = new List<string>();
        foreach (var type in CoreAssembly.GetTypes().Where(t => !IsPartOf(t, "BooleanEngine")))
        {
            var used = new List<Type>();
            used.AddRange(type.GetFields(all).Select(f => f.FieldType));
            used.AddRange(type.GetProperties(all).Select(p => p.PropertyType));
            foreach (var method in type.GetMethods(all).Cast<MethodBase>().Concat(type.GetConstructors(all)))
            {
                used.AddRange(method.GetParameters().Select(p => p.ParameterType));
                if (method is MethodInfo info)
                {
                    used.Add(info.ReturnType);
                }

                used.AddRange(method.GetMethodBody()?.LocalVariables.Select(v => v.LocalType) ?? []);
            }

            if (used.Any(UsesClipper2))
            {
                offenders.Add(type.FullName ?? type.Name);
            }
        }

        Assert.True(offenders.Count == 0, "Only BooleanEngine may use Clipper2: " + string.Join(", ", offenders));
    }

    private static bool IsPartOf(Type type, string name)
    {
        for (var current = type; current is not null; current = current.DeclaringType)
        {
            if (current.Name == name)
            {
                return true;
            }
        }

        return false;
    }

    private static bool UsesClipper2(Type type) =>
        type.Assembly.GetName().Name == "Clipper2"
        || type.Namespace?.StartsWith("Clipper2Lib", StringComparison.Ordinal) == true
        || (type.HasElementType && type.GetElementType() is { } element && UsesClipper2(element))
        || (type.IsGenericType && type.GetGenericArguments().Any(UsesClipper2));
}
