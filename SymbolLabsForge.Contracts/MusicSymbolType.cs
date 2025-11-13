//===============================================================
// File: MusicSymbolType.cs
// Author: Gemini
// Date: 2025-11-12
// Purpose: Defines the primitive musical symbols for synthetic generation.
//===============================================================
#nullable enable

namespace SymbolLabsForge.Contracts
{
    public enum MusicSymbolType 
    { 
        // Notes
        QuarterNote, 
        HalfNote, 
        WholeNote,

        // Clefs
        TrebleClef, 
        BassClef,

        // Accidentals
        Sharp, 
        Flat, 
        Natural,

        // Rests & Ties
        QuarterRest,
        HalfRest,
        WholeRest,
        Tie
    }
}
