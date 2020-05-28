//Mixture laws for metallic matrix composites
// From Ashby - 10.1016/0956-7151(93)90242-K
// From Shercliff and Ashby - 10.1179/mst.1994.10.6.443
// From F. Akhtar, Canadian Metallurgical Quarterly 2014 Vol.53 No 3 253

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
    [BindableDisplay(Name = "Matrix-Particles Halpin-Tsai ROM", Description = "Example model for metallic matrix composites, considering shape ratio of reinforcement.", GroupName = "Examples")]
    [Image("UserModel.model.png", "UserModel.thumbnail.png")]
    public class ExampleModel
    {
        public enum ShapeRatioType
        {
            [Display(Description = "Halpin Tsai")]
            HalpinTsai,
        }
        [Option]
        [Display(Name = "Mixture laws", Description = "Pick a model")]
        [Group("Mixture laws", 1)]
        public ShapeRatioType mixtureLaws = ShapeRatioType.HalpinTsai;
        
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

        [Variable("s")]
        [Display(Name = "Aspect Ratio", Description = "Specify the mean particles aspect ratio")]
        [RangeValues(Start = 1, End = 5, Number = 3, Logarithmic = false)]
        [Bounds(1, 1000)]
        [Group("Reinforcement description", 5)]
        public double aspectRatio;

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
            return HTRuleOfMixture(reinforcement.Price, matrix.Price);
        }

        [CalculatedData("Density", "kg/m^3")]
        public double Density()
        {
            return HTRuleOfMixture(reinforcement.Density, matrix.Density);
        }

        [CalculatedData("Young's modulus", "GPa")]
        public double YoungsModulus()
        {
            return HTRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
        }

        [CalculatedData("Flexural modulus", "GPa")]
        public double FlexuralModulus()
        {
            return HTRuleOfMixture(reinforcement.YoungsModulus, matrix.YoungsModulus);
        }

        [CalculatedData("Yield strength (elastic limit)", "GPa")]
        public double YieldStrength()
        {
            return HTRuleOfMixture(reinforcement.YieldStrength, matrix.YieldStrength);
        }

        [CalculatedData("Hardness - Vickers", "HV")]
        public double HardnessVickers()
        {
            return HTRuleOfMixture(reinforcement.HardnessVickers, matrix.HardnessVickers);
        }

        [CalculatedData("Specific heat capacity", "J/kg.°C")]
        public double SpecificHeat()
        {
            return HTRuleOfMixture(reinforcement.SpecificHeat, matrix.SpecificHeat);
        }

        [CalculatedData("Thermal conductivity", "W/m.°C")]
        public double ThermalConductivity()
        {
            return HTRuleOfMixture(reinforcement.ThermalConductivity, matrix.ThermalConductivity);
        }

        [CalculatedData("Thermal expansion coefficient", "µstrain/°C")]
        public double ThermalExpCoeff()
        {
            return HTRuleOfMixture(reinforcement.ThermalExpCoeff, matrix.ThermalExpCoeff);
        }

        [CalculatedData("Electrical resistivity", "µohm.cm")]
        public double ElecResistivity()
        {
            return HTRuleOfMixture(reinforcement.ElecResistivity, matrix.ElecResistivity);
        }

        private double HTRuleOfMixture(double a, double b)
        {
            var s = aspectRatio;
            var f = percentage / 100;
            var q = ((a / b) - 1) / ((a / b) + 2 * s);
            return b * (1 + 2 * s * q * f) / (1 - q * f);
        }
    }
}