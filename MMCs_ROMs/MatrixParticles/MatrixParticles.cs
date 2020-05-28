//Mixture laws for metallic matrix composites
// From Ashby - 10.1016/0956-7151(93)90242-K
// From Shercliff and Ashby - 10.1179/mst.1994.10.6.443

using System;
using System.ComponentModel.Composition;
using Granta.HybridBase;
using Granta.Framework.DataAnnotations;
using System.ComponentModel.DataAnnotations;

#region Steps to create a hybrid model

// Make sure you have a reference to both:
// System.ComponentModel.Composition (part of .Net framework)
// and
// Granta.HybridBase (found in the Selector install folder, eg c:\program files\CES Selector 2010)
//
// Create two public classes:
// * One to hold source material values (the 'Source Material')
// * One to return calculated values (the 'Model')
//
// To deploy your model, drop the built library (dll) into the plugins folder in the CES install folder.
// CES will automatically find your model and offer it as an option in the synthesizer user interface.

#endregion

namespace UserModel
{
    #region List of all properties used in model calculations

    // You must create a public class to store source material data for your synthesis.
    // To specify how these properties map to the database, mark up data values with the [Data] attribute, 
    // which requires a name and optional unit.

    #endregion

    public class SourceData
    {
        // When requesting price data, the unit takes the form currency/kg
        [Data("Price", "currency/kg")]
        public double Price;

        [Data("Density", "kg/m^3")]
        public double Density;

        [Data("Young's modulus", "GPa")]
        public double YoungsModulus;

        [Data("Flexural modulus", "GPa")]
        public double FlexuralModulus;

        [Data("Poisson's ratio", "")]
        public double PoissonCoeff;

        [Data("Yield strength (elastic limit)", "GPa")]
        public double YieldStrength;

        [Data("Hardness - Vickers", "HV")]
        public double HardnessVickers;

        [Data("Specific heat capacity", "J/kg.°C")]
        public double SpecificHeat;

        [Data("Thermal expansion coefficient", "µstrain/°C")]
        public double ThermalExpCoeff;

        [Data("Thermal conductivity", "W/m.°C")]
        public double ThermalConductivity;

        [Data("Electrical resistivity", "µohm.cm")]
        public double ElecResistivity;
    }

    #region Model name, images and description

    // In order to calculated a synthesis, you must create a public class that defines your model.
    // Mark the class with the [Export("Granta.HybridModel")] attribute
    // Add the [Model] attribute to provide a name and description.

    #endregion

    [Export("Granta.HybridModel")]
    [BindableDisplay(Name = "Matrix-Particles ROM", Description = "Example models for metallic matrix composites.", GroupName = "Examples")]
    [Image("UserModel.model.png", "UserModel.thumbnail.png")]
    public class ExampleModel
    {
        public enum ModelType
        {[Display(Description = "Voigt or Upper bound")] Voigt,
            [Display(Description = "Reuss or Lower bound")] Reuss,
            [Display(Description = "Voigt-Reuss-Hill or Mean")] VoigtReussHill }

        [Option]
        [Display(Name = "Mixture laws", Description = "Pick a model")]
        [Group("Mixture laws", 1)]
        public ModelType mixtureLaws = ModelType.VoigtReussHill;
        
        #region Details of Source Material inputs

        // Create one or more instances of the source material.
        // Make sure the materials are marked as public.
        // Use the [Material] attribute give the source material a name.
        // Here we create two materials  - a matrix and a reinforcement.

        #endregion

        [Material]
        [Display(Name = "Matrix", Description = "Matrix material")]
        [Group("Matrix", 2)]
        public SourceData matrix;

        [Material]
        [Display(Name = "Reinforcement", Description = "Reinforcement material")]
        [Group("Reinforcement", 3)]
        public SourceData reinforcement;

        #region Details of Model Variables and Model Parameter inputs

        // Variables are of type public double, and must be marked with the 
        // [Variable] attribute, which requires a name (used in the UI) and unit.
        // 
        // You can optionally specify default values by using either the 
        // [RangeValues] or [Values] attributes.
        // 
        // You can optionally specify an upper or lower bound by using the 
        // [Bounds] attribute

        #endregion

        [Variable("%")]
        [Display(Name = "Reinforcement percentage", Description = "Specify the volume fraction")]
        [RangeValues(Start = 15, End = 25, Number = 3, Logarithmic = false)]
        [Bounds(0, 100)]
        [Group("Reinforcement percentage", 4)]
        public double percentage;
        
        #region  Model Calculations

        // Create a method for each attribute you wish to calculate.
        // Each method should be public and return a double.
        //
        // Mark the method with the 
        // [CalculatedData] attribute, specifying the name and unit
        // of the data value you wish to calculate.
        // 
        // You can add as many methods as you require.

        #endregion

        [CalculatedData("Price", "currency/kg")]
        public double Price()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    return VoigtRuleOfMixture(reinforcement.Price, matrix.Price);
                case ModelType.Reuss:
                    return ReussRuleOfMixture(reinforcement.Price, matrix.Price);
                case ModelType.VoigtReussHill:
                    var a = VoigtRuleOfMixture(reinforcement.Price, matrix.Price);
                    var b = ReussRuleOfMixture(reinforcement.Price, matrix.Price);
                    return VRHRuleOfMixture(a,b);
                default:
                    return double.NaN;
            }
        }
        [CalculatedData("Density", "kg/m^3")]
        public double Density()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    return VoigtRuleOfMixture(reinforcement.Density, matrix.Density);
                case ModelType.Reuss:
                    return ReussRuleOfMixture(reinforcement.Density, matrix.Density);
                case ModelType.VoigtReussHill:
                    var a = VoigtRuleOfMixture(reinforcement.Density, matrix.Density);
                    var b = ReussRuleOfMixture(reinforcement.Density, matrix.Density);
                    return VRHRuleOfMixture(a, b);
                default:
                    return double.NaN;
            }
        }
        [CalculatedData("Young's modulus", "GPa")]
        public double YoungsModulus()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    return VoigtRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                case ModelType.Reuss:
                    return ReussRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                case ModelType.VoigtReussHill:
                    var a = VoigtRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                    var b = ReussRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                    return VRHRuleOfMixture(a, b);
                default:
                    return double.NaN;
            }
        }
        [CalculatedData("Flexural modulus", "GPa")]
        public double FlexuralModulus()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    return VoigtRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                case ModelType.Reuss:
                    return ReussRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                case ModelType.VoigtReussHill:
                    var a = VoigtRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                    var b = ReussRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
                    return VRHRuleOfMixture(a, b);
                default:
                    return double.NaN;
            }
        }
        [CalculatedData("Yield strength (elastic limit)", "GPa")]
        public double YieldStrength()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    return VoigtRuleOfMixture(reinforcement.YieldStrength, matrix.YieldStrength);
                case ModelType.Reuss:
                    return LBYSRuleOfMixture(reinforcement.YieldStrength, matrix.YieldStrength);
                case ModelType.VoigtReussHill:
                    var a = VoigtRuleOfMixture(reinforcement.YieldStrength, matrix.YieldStrength);
                    var b = LBTCRuleOfMixture(reinforcement.YieldStrength, matrix.YieldStrength);
                    return VRHRuleOfMixture(a, b);
                default:
                    return double.NaN;
            }
        }
        [CalculatedData("Hardness - Vickers", "HV")]
        public double HardnessVickers()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    return VoigtRuleOfMixture(reinforcement.HardnessVickers, matrix.HardnessVickers);
                case ModelType.Reuss:
                    return ReussRuleOfMixture(reinforcement.HardnessVickers, matrix.HardnessVickers);
                case ModelType.VoigtReussHill:
                    var a = VoigtRuleOfMixture(reinforcement.HardnessVickers, matrix.HardnessVickers);
                    var b = ReussRuleOfMixture(reinforcement.HardnessVickers, matrix.HardnessVickers);
                    return VRHRuleOfMixture(a, b);
                default:
                    return double.NaN;
            }
        }
        [CalculatedData("Specific heat capacity", "J/kg.°C")]
        public double SpecificHeat()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    var cprV = reinforcement.Density * reinforcement.SpecificHeat;
                    var cpmV = matrix.Density * matrix.SpecificHeat;
                    var dVoigt = VoigtRuleOfMixture(reinforcement.Density, matrix.Density);
                    return VoigtRuleOfMixture(cprV, cpmV)/ dVoigt;
                case ModelType.Reuss:
                    var dReuss = ReussRuleOfMixture(reinforcement.Density, matrix.Density);
                    var cprR = reinforcement.Density * reinforcement.SpecificHeat;
                    var cpmR = matrix.Density * matrix.SpecificHeat;
                    return VoigtRuleOfMixture(cprR, cpmR) / dReuss;
                case ModelType.VoigtReussHill:
                    var dVRH = ReussRuleOfMixture(reinforcement.Density, matrix.Density);
                    var cprVRH = reinforcement.Density * reinforcement.SpecificHeat;
                    var cpmVRH = matrix.Density * matrix.SpecificHeat;
                    return VRHRuleOfMixture(cprVRH, cpmVRH) / dVRH;
                default:
                    return double.NaN;
            }
        }
        [CalculatedData("Thermal conductivity", "W/m.°C")]
        public double ThermalConductivity()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    return VoigtRuleOfMixture(reinforcement.ThermalConductivity, matrix.ThermalConductivity);
                case ModelType.Reuss:
                    return LBTCRuleOfMixture(reinforcement.ThermalConductivity, matrix.ThermalConductivity);
                case ModelType.VoigtReussHill:
                    var a = VoigtRuleOfMixture(reinforcement.ThermalConductivity, matrix.ThermalConductivity);
                    var b = LBTCRuleOfMixture(reinforcement.ThermalConductivity, matrix.ThermalConductivity);
                    return VRHRuleOfMixture(a, b);
                default:
                    return double.NaN;
            }
        }
        [CalculatedData("Thermal expansion coefficient", "µstrain/°C")]
        public double ThermalExpCoeff()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    var Levin = LevinRuleOfMixture(reinforcement.ThermalExpCoeff, matrix.ThermalExpCoeff, reinforcement.YoungsModulus, matrix.YoungsModulus);
                    return SchaperyRuleOfMixture(reinforcement.ThermalExpCoeff, matrix.ThermalExpCoeff, Levin, reinforcement.PoissonCoeff, matrix.PoissonCoeff);
                case ModelType.Reuss:
                    return LevinRuleOfMixture(reinforcement.ThermalExpCoeff, matrix.ThermalExpCoeff, reinforcement.YoungsModulus, matrix.YoungsModulus);
                case ModelType.VoigtReussHill:
                    var b = LevinRuleOfMixture(reinforcement.ThermalConductivity, matrix.ThermalConductivity, reinforcement.YoungsModulus, matrix.YoungsModulus);
                    var a = SchaperyRuleOfMixture(reinforcement.ThermalExpCoeff, matrix.ThermalExpCoeff, b, reinforcement.PoissonCoeff, matrix.PoissonCoeff);
                    return VRHRuleOfMixture(a, b);
                default:
                    return double.NaN;
            }
        }
        [CalculatedData("Electrical resistivity", "µohm.cm")]
        public double ElecResistivity()
        {
            switch (mixtureLaws)
            {
                case ModelType.Voigt:
                    return VoigtRuleOfMixture(reinforcement.ElecResistivity, matrix.ElecResistivity);
                case ModelType.Reuss:
                    return ReussRuleOfMixture(reinforcement.ElecResistivity, matrix.ElecResistivity);
                case ModelType.VoigtReussHill:
                    var a = VoigtRuleOfMixture(reinforcement.ElecResistivity, matrix.ElecResistivity);
                    var b = ReussRuleOfMixture(reinforcement.ElecResistivity, matrix.ElecResistivity);
                    return VRHRuleOfMixture(a, b);
                default:
                    return double.NaN;
            }
        }
        private double VoigtRuleOfMixture(double a, double b)
        {
            var f = percentage / 100;
            return f * a + (1.0 - f) * b;
        }
        private double ReussRuleOfMixture(double a, double b)
        {
            var f = percentage / 100;
            return 1 / (((f / a) + ((1.0 - f) / b)));
        }
        private double LBYSRuleOfMixture(double a, double b)
        {
            var f = percentage / 100; 
            return b * ( 1+ (1/16) * ((Math.Pow(f, 0.5))/ (1 - Math.Pow(f, 0.5))));
        }
        private double LBTCRuleOfMixture(double a, double b)
        {
            var f = percentage / 100;
            return b * ((a + (2 * b) - (2 * f * (b - a))) / ((a + (2 * b) + (f * (b - a)))));
        }
        private double SchaperyRuleOfMixture(double a, double b, double c, double d, double e)
        {
            var f = percentage / 100;
            return (f * a * (1 + d)) + ((1 - f) * b * (1 + e)) - (c * ((f * d) + ((1 - f) * e)));
        }
        private double LevinRuleOfMixture(double a, double b, double c, double d)
        {
            var f = percentage / 100;
            return ((c * a * f) + (d * b * (1 - f))) / ((c * f) + (d * (1 - f)));
        }
        private double VRHRuleOfMixture(double a, double b)
        {
            return (a + b) / 2;
        }
    }
}