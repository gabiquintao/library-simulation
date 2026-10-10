# Learning

## Types and Memory
- **`struct` vs `class`:** a class variable holds a reference to an object on the heap; a struct holds 
its data where the variable lives (stack for locals, inside the array or object for fields). 
Copying a struct copies the the data. An array of struct is one contiguous block, which is 
cache-friendly and can be written to disk in one go.
- **Blittable Types:** structs containing only fixed-size primitives (no string or references).
Required to dump memory directly to a file.
- **`readonly struct`:** all fields are immutable after creation. Properties use get and init (settable
only at object initialization).
- **`record struct:`** the compiler generates value equality, `GetHashCode`, `ToString` and `with`
expressions. It does not change size or layoyt.
- **`with` Expression:** creates a copy of an object, updating only the specific properties chosen, while
keeping the original object untouched.
- **Strongly Typed IDs:** a one-field `readonly record struct` wrapper (for example around an `int`) has
the same size as the primitive but the compiler stops one kind of id being passed as another. Also works as
a `Dictionary` key without boxing (storing a value type inside a heap object), keeping lookups
allocation-free.
- **`enum : byte`:** sets the backing type, so the enum takes 1 byte instead of 4.
- **`sealed`:** the class cannot be inherited. Communicates intent and lets the JIT devirtualize calls.

## Struct Layout
- **`[StructLayout(LayoutKind.Sequential)]`** fields are laid out in declaration order. For structs of primitives this is already
the default; the attribute documents that field order matters. `Auto` lets the runtime reorder fields.
- **Padding:** the compiler inserts empty bytes so each fields sits at an address that is a multiple of its
size, and pads the struct's total size to a multiple of its largest field. Ordering fields from largest
to smallest minimizes it.
- **Integer Sizes:** `byte` 1, `ushort` 2, `int` 4, `long` 8 bytes. Choosing by the size needed, not by
wheter the value can be negative.
- **Signed vs Unsinged:** `int` is the .NET standard for counts and indexes (`Length`, `Count`, indexers).
Unsigned substraction wraps around instead of going negative, and unsigned types remove `-1` as a sentinel.
Unsigned is useful for bit manipulation, hashes and compact fields.
- **Little-endian:** multi-byte numbers are stored with the least significant byte first, so `1899`
(`0x076B`) appears as `6B 07` in a hex dump.
- **`Unsafe.SizeOf<T>()`**: prints the real size of a struct, useful to check padding.
- **RuntimeHelpers.IsReferenceOrContainsReferences<T>()**: tells whether a type contains references, useful
to guard the blittable property in a test.

## Span and Low-level Memory
- **`Span<T>`:** a window existing memory (an array, part of an array, stack memory). No copy; writing
through it changes the original.
- **`ReadOnlySpan<T>`:** the read-only version.
- **`AspSpan()` / `AsSpan(start, length)`:** crates a span over an array or a part of it.
- **`MemoryMarshall.AsBytes`:** reinterprets a span of structs as a span of bytes, without copying. This is
what allows writing a whole struct array to disk with a single call.
- **`Convert.ToHexString`:** formats bytes as a hex string, used to build a hex dump.
- **`{value:X4}`:** interpolated-string format for hexadecimal with at least 4 digits.
- **`where T : unmanaged`:** generic constraint that only accepts blittable types.

## Text and Encodin
- A `string` is a heap reference (UTF-16 in memory), so it cannot be stored in a blittable struct.
- **UTF-8 byte length differs from character count:** accented characters take 2 bytes. Always store
lengths in bytes.
- **Offset + length into a shared byte buffer:** replaces strings in structs. Positions stay valid when the
buffer is reallocated or written to disk, unlike addresses.
- **`Encoding.UTF8.GetBytes` / `GetString`:** convert between strings and bytes.

## Arrays and Growth
- An array has a fixed size; `Array.Resize` allocates a new array and copies. The `ref` parameter is needed
so the variable itself can be repoited.
- **Capacity and Count:** `Length` is capacity, a separate counter holds the numbers of used elements.
- **Doubling** on growth gives amortized constant cost per insert (total copies below 2N); growing by a
fixed step gives quadratic total cost. `List<T>` doubles.
- A new array is zero-filled, so "0 means empty/none" needs no itilization (trick: store index + 1 so
that 0 menas "no value").
- **`readonly` fields:** the reference cannot be reassigned, but the contents can change. A field that gets
- reassigned (for example by `Array.Resize` or `++`) cannot be `readonly`.

## Validation and Errors
- Exceptions for programmer errors, result values for expected situations.
- Value types are never null, so only reference parameters (such as `string`) need null checks.
- Validate first, mutate after, so a failed call leaves no partial state. An exception jumps out of the
method immediately, skipping the remaining lines.
- **Atomaticity:** an operation either fully happens or not at all (the "A" in ACID). Databases achieve
it with rollback or write-ahead logging.

# Language Features
- Tuple returns values with named elements and deconstrution.
- Object initializers.
- Collection expressions.
- Primary-style `record struct`, syntax: `record struct Id(int Value)`.
- Expression-bodied members: `int Total => _count`.
- Local functions and static helper methods.
- `Math.Max`, `Math.Min` for clamping.
- Passing time as a parameter (or `TimeProvider`) instead of reading the clock, for deterministic tests and
log replay.

## Data Structures
- Store ids, not copies, in secondary structures.
- Rows (array of structs) are good for "fetch one whole record"; columns (struct of arrays) are good for
scanning one field over many records.
- Fixed-size records allow direct addressing: `position = row * recordSize + fieldOffset`. Variable-size
data (strings) breaks this, hence the separate text buffer.

## Persistence Concepts
- **Snapshot:** dump of the full state to a file. Fast to load when the data is (read bytes straight in
to the array).
- **File header:** magic number, format version and counts, so a file can be recognized, versioned and read
without guessing sizes.
- **Atomic file replace:** write to a temporary file, flush to disk, then rename over the real file. Rename
is atomic.
- **