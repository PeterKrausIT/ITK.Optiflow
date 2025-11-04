# ITK.OptiFlow

## Overview
OptiFlow is a middleware project designed to manage data workflows in the optical industry. It serves as an intermediary system for routing and converting lens orders and other messages between different optical industry platforms and formats.

## Purpose
The primary purpose of this middleware is to facilitate seamless communication between various optical industry systems by:

- Routing orders from multiple input sources to appropriate destination systems
- Converting lens order data between different formats used in the optical industry
- Standardizing communication protocols for lens manufacturing and processing systems
- Managing workflow processes for lens order fulfillment

## Key Components

The ITK.OptiFlow system consists of several key modules:

- **Adapters**: Connect to external systems via FTP, WebService, email, etc
  - `MB.Adapter.GetFTP`
  - `MB.Adapter.SetFTP

- **Parsers**: Convert incoming data formats into standardized internal formats
  - `MB.Parser.OMA`: Parses OMA format orders with detailed lens specifications

- **Generators**: Create output formats for target systems
  - `MB.Generator.B2BOrder`: Generates standardized B2B optical format orders
  - `MB.Generator.PlainText`: Produces human-readable order formats

- **Transformations**: Apply business rules and transformations to order data
  - `MB.Transformation.RuleEngine`: Manages rules for data conversion and validation

## How It Works

1. Orders arrive from various sources (labs, retail locations, online systems)
2. The system parses incoming orders into a standardized internal format
3. Transformation rules are applied based on business requirements
4. The order is then converted to the appropriate output format
5. Finally, the transformed order is routed to the destination system

## Intended Use

This middleware is intended for optical labs, lens manufacturers, and optical retail networks that need to:
- Integrate multiple ordering systems
- Process lens orders from various sources
- Standardize order formats across the enterprise
- Route orders to appropriate processing facilities
