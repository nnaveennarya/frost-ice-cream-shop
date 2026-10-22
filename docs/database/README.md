# Frost database diagrams

- `Frost-shop-ER.png`: export showing the five application tables and their database-enforced relationships. Suitable for attaching directly to the submission document.
- `Frost-full-schema.mmd`: editable Mermaid ER source covering all 18 tables, including OAuth, sessions, cache, queue, and migration bookkeeping. Open it in a Mermaid-capable editor to edit or export the full diagram.

The schema was compared with the supplied Laravel migrations and extracted from CREATE TABLE declarations in the demonstration database snapshot. No customer records, account values, tokens, or other row data are included.

Solid relationships are enforced foreign keys: each item belongs to one category (delete restricted), and an audit event may retain an optional user reference (set to NULL on user deletion). Dashed relationships in the full source are logical associations; the migrations do not declare foreign key constraints for those OAuth/session columns. `oauth_clients.owner_type` and `owner_id` form an optional polymorphic owner reference, not a fixed users foreign key. Audit `entity_type` and `entity_id` identify an item or customer without a database constraint, allowing the history to survive record deletion. Customers have no direct relationship to inventory in this schema.

The refresh-token and device-code tables exist as framework schema, although this application enables only the authorization-code grant and disables refresh tokens. Tables are documented as implemented, including nullable timestamps and unique keys. `items.price >= 0` and `items.version >= 1`, `customers.version >= 1` are database check constraints. Inventory quantity and version fields use unsigned integers.
